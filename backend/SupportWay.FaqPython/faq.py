from __future__ import annotations

import json
import logging
import os
import re
import unicodedata
from typing import Any, Iterable

try:
    from dotenv import load_dotenv
except Exception:
    load_dotenv = None

if load_dotenv:
    load_dotenv()

try:
    from google import genai
except Exception:
    genai = None

from dbtools import SupportWayDataTools, build_engine
from question_base import Action, DB_INTENTS, FAQ_ENTRIES, FaqEntry, ROUTER_PROMPT, TYPO_MAP

logger = logging.getLogger(__name__)

GEMINI_API_KEY = os.getenv('GEMINI_API_KEY', '').strip()
GEMINI_MODEL = os.getenv('GEMINI_MODEL', 'gemini-2.5-flash').strip()
GEMINI_CLIENT = None

if genai is not None and GEMINI_API_KEY:
    try:
        GEMINI_CLIENT = genai.Client(api_key=GEMINI_API_KEY)
    except Exception as exc:
        logger.warning('Gemini init failed: %s', exc)
        GEMINI_CLIENT = None

TOKEN_RE = re.compile(r'[\wа-яіїєґ]+', re.I | re.U)
_ENGINE = None
_TOOLS: SupportWayDataTools | None = None


def get_tools() -> SupportWayDataTools | None:
    global _ENGINE, _TOOLS
    if _TOOLS is not None:
        return _TOOLS
    try:
        _ENGINE = build_engine()
        _TOOLS = SupportWayDataTools(_ENGINE)
        return _TOOLS
    except Exception as exc:
        logger.warning('Database tools unavailable: %s', exc)
        return None


def database_health() -> dict[str, Any]:
    tools = get_tools()
    if tools is None:
        return {"status": "error", "detail": "Database tools are not initialized"}
    health = tools.health()
    return {"status": health.status, "detail": health.detail}


def normalize_text(text: str) -> str:
    value = unicodedata.normalize('NFKC', text or '').lower().strip()
    value = value.replace('ʼ', "'").replace('`', "'").replace('ё', 'е')
    value = re.sub(r'[^\wа-яіїєґa-z0-9\s\-\']+', ' ', value, flags=re.I | re.U)
    tokens = [TYPO_MAP.get(token, token) for token in value.split()]
    return re.sub(r'\s+', ' ', ' '.join(tokens)).strip()


def tokenize(text: str) -> list[str]:
    return [m.group(0) for m in TOKEN_RE.finditer(normalize_text(text)) if len(m.group(0)) >= 2]


def role_set(roles: Iterable[str] | None) -> set[str]:
    return {role.strip().lower() for role in roles or [] if role and role.strip()}


def available_entries(roles: Iterable[str] | None) -> list[FaqEntry]:
    roles_lower = role_set(roles)
    return [entry for entry in FAQ_ENTRIES if not entry.roles or any(role in roles_lower for role in entry.roles)]


def score(entry: FaqEntry, question: str, question_tokens: list[str]) -> float:
    q = normalize_text(question)
    result = 0.0
    for phrase in entry.questions:
        p = normalize_text(phrase)
        if p and p in q:
            result += 5.0
        phrase_tokens = set(tokenize(phrase))
        result += sum(1 for token in question_tokens if token in phrase_tokens) * 1.45
    result += sum(1 for token in tokenize(entry.category) if token in question_tokens) * 1.8
    return result


def detect_db_intent(question: str) -> tuple[str | None, float]:
    normalized = normalize_text(question)
    tokens = set(tokenize(normalized))
    best_intent: str | None = None
    best_score = 0.0

    for intent, meta in DB_INTENTS.items():
        current = 0.0
        for phrase in meta['keywords']:
            p = normalize_text(phrase)
            if p and p in normalized:
                current += 5.0
            phrase_tokens = set(tokenize(phrase))
            current += len(tokens.intersection(phrase_tokens)) * 1.6
        if current > best_score:
            best_score = current
            best_intent = intent

    if best_score >= 2.5:
        return best_intent, min(0.95, best_score / 10)

    gemini_intent = gemini_route_intent(question)
    if gemini_intent:
        return gemini_intent, 0.88

    return None, 0.0


def gemini_route_intent(question: str) -> str | None:
    if GEMINI_CLIENT is None:
        return None
    try:
        intents = json.dumps({key: value['keywords'] for key, value in DB_INTENTS.items()}, ensure_ascii=False)
        prompt = ROUTER_PROMPT.format(intents=intents) + f"\n\nПитання: {question}"
        response = GEMINI_CLIENT.models.generate_content(model=GEMINI_MODEL, contents=prompt)
        text = getattr(response, 'text', '') or ''
        match = re.search(r'\{.*\}', text, flags=re.S)
        if not match:
            return None
        payload = json.loads(match.group(0))
        intent = str(payload.get('intent') or '').strip()
        return intent if intent in DB_INTENTS else None
    except Exception as exc:
        logger.warning('Gemini intent routing failed: %s', exc)
        return None


def gemini_rerank(question: str, candidates: list[dict[str, Any]]) -> int | None:
    if GEMINI_CLIENT is None or not candidates:
        return None
    try:
        prompt = (
            'Ти semantic router для FAQ бота платформи SupportWay. '
            'Обери індекс найкращої категорії для питання користувача. '
            'Поверни тільки JSON: {"index": number}.\n\n'
            f'Питання: {question}\n\n'
            f'Кандидати: {json.dumps(candidates, ensure_ascii=False)}'
        )
        response = GEMINI_CLIENT.models.generate_content(model=GEMINI_MODEL, contents=prompt)
        text = getattr(response, 'text', '') or ''
        match = re.search(r'\{.*\}', text, flags=re.S)
        if not match:
            return None
        payload = json.loads(match.group(0))
        index = int(payload.get('index'))
        if 0 <= index < len(candidates):
            return index
    except Exception as exc:
        logger.warning('Gemini rerank failed: %s', exc)
    return None


def to_response(entry: FaqEntry, confidence: float, entries: list[FaqEntry], source: str = 'faq') -> dict[str, Any]:
    suggestions = [e.main_question for e in entries if e.category != entry.category][:4]
    return {
        'answer': entry.answer,
        'category': entry.category,
        'confidence': round(max(0.0, min(confidence, 0.99)), 2),
        'actions': [{'label': action.label, 'route': action.route} for action in entry.actions],
        'suggestions': suggestions,
        'source': source,
    }


def format_money(value: float | int | None) -> str:
    amount = float(value or 0)
    return f"{amount:,.2f}".replace(',', ' ').replace('.', ',')


def answer_from_database(intent: str, confidence: float) -> dict[str, Any]:
    tools = get_tools()
    if tools is None or tools.health().status != 'ok':
        return {
            'answer': 'Я зрозумів, що потрібна аналітика з бази даних, але зараз Python-бот не має підключення до PostgreSQL. Перевір SUPPORTWAY_DB_CONNECTION_STRING у .env та чи запущена база SupportWay.',
            'category': 'База даних недоступна',
            'confidence': 0.2,
            'actions': [],
            'suggestions': ['скільки зібрали коштів', 'скільки всього запитів', 'статистика платформи'],
            'source': 'database_unavailable',
        }

    try:
        if intent == 'money_stats':
            data = tools.get_money_stats()
            answer = (
                f"За даними БД SupportWay зібрано {format_money(data['collected_amount'])} грн у запитах допомоги. "
                f"Загальна ціль зборів: {format_money(data['target_amount'])} грн. "
                f"Прогрес зборів: {data['completion_percent']}%. "
                f"У таблиці Payments зафіксовано {data['payments_count']} платежів на суму {format_money(data['payments_amount'])} грн."
            )
            return _db_response(answer, 'Аналітика коштів', confidence, intent)

        if intent == 'help_request_stats':
            data = tools.get_help_request_stats()
            answer = (
                f"У системі створено {data['total_requests']} запитів допомоги, з них активних — {data['active_requests']}. "
                f"На карті мають координати {data['mapped_requests']} запитів. "
                f"Повністю або понад ціль закрито {data['completed_requests']} зборів. "
                f"У запитах додано {data['request_items_count']} конкретних потреб у {data['support_types_count']} типах допомоги."
            )
            return _db_response(answer, 'Аналітика запитів', confidence, intent, actions=[Action('Відкрити запити', '/requests'), Action('Відкрити карту', '/map')])

        if intent == 'user_stats':
            data = tools.get_user_stats()
            answer = (
                f"У БД є {data['total_users']} користувачів і {data['profiles']} профілів. "
                f"Верифікованих профілів — {data['verified_profiles']}. "
                f"Волонтерів — {data['volunteers']}, військових — {data['military']}. "
                f"Також є {data['follows']} підписок і {data['profile_ratings']} оцінок профілів."
            )
            return _db_response(answer, 'Аналітика користувачів', confidence, intent)

        if intent == 'content_stats':
            data = tools.get_content_stats()
            answer = (
                f"Контент у SupportWay: {data['posts']} звичайних публікацій, {data['help_requests']} запитів допомоги, "
                f"{data['likes']} лайків і {data['comments']} коментарів. "
                f"У PostgreSQL також є {data['chats']} чатів і {data['postgres_messages']} старих повідомлень, якщо вони ще не перенесені повністю в MongoDB."
            )
            return _db_response(answer, 'Аналітика контенту', confidence, intent)

        if intent == 'verification_stats':
            data = tools.get_verification_stats()
            answer = (
                f"Заявок на верифікацію всього: {data['total']}. "
                f"Очікують рішення: {data['pending']}, схвалено: {data['approved']}, відхилено: {data['rejected']}. "
                f"Заявок на волонтера: {data['volunteer_requests']}, на військового: {data['military_requests']}."
            )
            return _db_response(answer, 'Аналітика верифікацій', confidence, intent, actions=[Action('Відкрити адмін-панель', '/admin')])

        if intent == 'top_help_requests':
            rows = tools.get_top_help_requests(5)
            if not rows:
                answer = 'У БД поки немає запитів допомоги з даними про зібрані кошти.'
            else:
                lines = [
                    f"{idx + 1}. @{row['username']}: {format_money(row['collected_amount'])} / {format_money(row['target_amount'])} грн ({row['completion_percent']}%) — {row['content']}"
                    for idx, row in enumerate(rows)
                ]
                answer = 'Топ запитів за зібраною сумою:\n' + '\n'.join(lines)
            return _db_response(answer, 'Топ зборів', confidence, intent, actions=[Action('Відкрити запити', '/requests')])

        if intent == 'recent_help_requests':
            rows = tools.get_recent_help_requests(5)
            if not rows:
                answer = 'У БД поки немає запитів допомоги.'
            else:
                lines = [
                    f"{idx + 1}. @{row['username']}: {row['content']} — ціль {format_money(row['target_amount'])} грн, зібрано {format_money(row['collected_amount'])} грн"
                    for idx, row in enumerate(rows)
                ]
                answer = 'Останні запити допомоги:\n' + '\n'.join(lines)
            return _db_response(answer, 'Останні запити', confidence, intent, actions=[Action('Відкрити запити', '/requests')])

        if intent == 'known_tables':
            tables = tools.list_known_tables()
            useful = ', '.join(tables[:40])
            answer = f"Я бачу такі таблиці в public schema SupportWay: {useful}. Основні для аналітики: HelpRequests, Payments, Posts, PostLikes, PostComments, AspNetUsers, Profiles, VerificationRequests, RequestItems, SupportTypes."
            return _db_response(answer, 'Таблиці БД', confidence, intent)

        data = tools.get_dashboard_kpi()
        money = data['money']
        users = data['users']
        content = data['content']
        verification = data['verification']
        answer = (
            f"Ключові KPI SupportWay: зібрано {format_money(money['collected_amount'])} грн із цілі {format_money(money['target_amount'])} грн; "
            f"запитів допомоги — {money['total_requests']}, активних — {money['active_requests']}; "
            f"користувачів — {users['total_users']}, верифікованих — {users['verified_profiles']}; "
            f"публікацій — {content['posts']}, лайків — {content['likes']}, коментарів — {content['comments']}; "
            f"заявок на верифікацію очікують — {verification['pending']}."
        )
        return _db_response(answer, 'Аналітика платформи', confidence, intent)

    except Exception as exc:
        logger.exception('Database answer failed')
        return {
            'answer': f'Не вдалося виконати вибірку з БД. Перевір, чи застосовані міграції та чи таблиці мають актуальну структуру. Деталь: {exc}',
            'category': 'Помилка БД',
            'confidence': 0.1,
            'actions': [],
            'suggestions': ['які таблиці в бд', 'статистика платформи'],
            'source': 'database_error',
        }


def _db_response(answer: str, category: str, confidence: float, intent: str, actions: list[Action] | None = None) -> dict[str, Any]:
    return {
        'answer': answer,
        'category': category,
        'confidence': round(max(0.0, min(confidence, 0.99)), 2),
        'actions': [{'label': action.label, 'route': action.route} for action in (actions or [])],
        'suggestions': ['скільки зібрали коштів', 'скільки всього запитів', 'статистика платформи'],
        'source': f'database:{intent}',
    }


def answer_question(question: str, roles: Iterable[str] | None = None) -> dict[str, Any]:
    entries = available_entries(roles)
    q = (question or '').strip()
    if not q:
        return {
            'answer': 'Напишіть питання про реєстрацію, верифікацію, запити допомоги, донати, карту, публікації, чати або аналітику платформи.',
            'category': 'Підказка',
            'confidence': 0.0,
            'actions': [],
            'suggestions': [entry.main_question for entry in entries[:5]],
            'source': 'empty',
        }

    db_intent, db_confidence = detect_db_intent(q)
    if db_intent:
        result = answer_from_database(db_intent, db_confidence)
        _log_question(q, result, db_intent, result.get('source'))
        return result

    tokens = tokenize(q)
    ranked = sorted(
        ((entry, score(entry, q, tokens)) for entry in entries),
        key=lambda item: item[1],
        reverse=True,
    )

    top_entries = [entry for entry, _ in ranked[:5]]
    candidates = [
        {'index': idx, 'category': entry.category, 'questions': list(entry.questions[:4])}
        for idx, entry in enumerate(top_entries)
    ]
    selected_index = gemini_rerank(q, candidates)
    if selected_index is not None:
        selected = top_entries[selected_index]
        result = to_response(selected, 0.92, entries, source='gemini_rerank')
        _log_question(q, result, selected.category, result.get('source'))
        return result

    if not ranked or ranked[0][1] <= 0:
        result = {
            'answer': 'Я не знайшов точної відповіді, але можу допомогти з реєстрацією, верифікацією, запитами допомоги, донатами, картою, постами, чатами або аналітикою БД. Спробуйте сформулювати питання трохи інакше.',
            'category': 'Не знайдено',
            'confidence': 0.0,
            'actions': [],
            'suggestions': [entry.main_question for entry in entries[:5]] + ['скільки зібрали коштів', 'статистика платформи'],
            'source': 'not_found',
        }
        _log_question(q, result, None, result.get('source'))
        return result

    best, best_score = ranked[0]
    confidence = best_score / max(5, len(tokens) + 4)
    result = to_response(best, confidence, entries, source='keyword')
    _log_question(q, result, best.category, result.get('source'))
    return result


def get_suggestions(roles: Iterable[str] | None = None) -> list[dict[str, str]]:
    static = [{'text': entry.main_question, 'category': entry.category} for entry in available_entries(roles)]
    analytics = [
        {'text': 'скільки зібрали коштів', 'category': 'Аналітика коштів'},
        {'text': 'скільки всього запитів', 'category': 'Аналітика запитів'},
        {'text': 'статистика платформи', 'category': 'Аналітика платформи'},
        {'text': 'топ зборів', 'category': 'Топ зборів'},
    ]
    return analytics + static


def get_recent_questions(limit: int = 20) -> list[dict[str, Any]]:
    tools = get_tools()
    if tools is None:
        return []
    return tools.get_recent_faq_questions(limit)


def get_database_dashboard() -> dict[str, Any]:
    tools = get_tools()
    if tools is None:
        return {"available": False}
    if tools.health().status != 'ok':
        return {"available": False}
    return {"available": True, "data": tools.get_dashboard_kpi()}


def debug_answer(question: str, roles: Iterable[str] | None = None) -> dict[str, Any]:
    entries = available_entries(roles)
    tokens = tokenize(question)
    ranked = sorted(
        [{'category': entry.category, 'score': score(entry, question, tokens), 'mainQuestion': entry.main_question} for entry in entries],
        key=lambda item: item['score'],
        reverse=True,
    )
    db_intent, db_conf = detect_db_intent(question)
    return {
        'question': question,
        'normalized': normalize_text(question),
        'tokens': tokens,
        'llmEnabled': GEMINI_CLIENT is not None,
        'database': database_health(),
        'dbIntent': db_intent,
        'dbConfidence': db_conf,
        'ranking': ranked,
        'response': answer_question(question, roles),
    }


def _log_question(question: str, result: dict[str, Any], intent: str | None, source: str | None) -> None:
    tools = get_tools()
    if tools is None:
        return
    try:
        tools.log_faq_question(
            question=question,
            answer=str(result.get('answer') or ''),
            intent=intent,
            domain=str(result.get('category') or '') or None,
            source=source,
            confidence=float(result.get('confidence') or 0),
        )
    except Exception:
        return
