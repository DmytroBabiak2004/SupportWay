from __future__ import annotations

import logging
import os
from typing import Any, List

from fastapi import FastAPI, HTTPException, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

from faq import (
    GEMINI_CLIENT,
    answer_question,
    database_health,
    debug_answer,
    get_database_dashboard,
    get_recent_questions,
    get_suggestions,
)

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s [%(levelname)s] %(name)s: %(message)s'
)

logger = logging.getLogger('supportway-faq')


def get_cors_origins() -> list[str]:
    raw = os.getenv(
        'SUPPORTWAY_CORS_ORIGINS',
        'http://localhost:4200,http://localhost:5233'
    )

    return [
        origin.strip()
        for origin in raw.split(',')
        if origin.strip()
    ]


app = FastAPI(
    title='SupportWay Python FAQ Assistant',
    version='2.1.0',
    description='Python/FastAPI assistant for SupportWay with Gemini routing and PostgreSQL analytics.',
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=get_cors_origins(),
    allow_credentials=True,
    allow_methods=['*'],
    allow_headers=['*'],
)


class FaqQuickActionDto(BaseModel):
    label: str
    route: str


class FaqRequestDto(BaseModel):
    question: str = Field(default='')
    roles: List[str] = Field(default_factory=list)


class FaqResponseDto(BaseModel):
    answer: str
    category: str
    confidence: float = Field(default=0.0, ge=0.0, le=1.0)
    actions: List[FaqQuickActionDto] = Field(default_factory=list)
    suggestions: List[str] = Field(default_factory=list)
    source: str | None = None


class FaqSuggestionDto(BaseModel):
    text: str
    category: str


@app.get('/health')
def health() -> dict[str, Any]:
    db = database_health()
    db_ok = db.get('status') == 'ok'

    return {
        'status': 'ok',
        'version': '2.1.0',
        'llm': GEMINI_CLIENT is not None,
        'database': 'available' if db_ok else 'unavailable',
        'databaseDetail': db.get('detail'),
        'analytics': db_ok,
    }


@app.post('/faq', response_model=FaqResponseDto)
def ask_faq(body: FaqRequestDto):
    question = body.question.strip()

    if not question:
        raise HTTPException(status_code=400, detail='Question cannot be empty')

    try:
        return answer_question(question, body.roles)
    except Exception as exc:
        logger.exception('FAQ error')
        raise HTTPException(status_code=500, detail='Failed to answer FAQ') from exc


@app.post('/faq/debug')
def ask_debug(body: FaqRequestDto):
    question = body.question.strip()

    if not question:
        raise HTTPException(status_code=400, detail='Question cannot be empty')

    try:
        return debug_answer(question, body.roles)
    except Exception as exc:
        logger.exception('FAQ debug error')
        raise HTTPException(status_code=500, detail='Failed to debug FAQ') from exc


@app.get('/faq/suggestions', response_model=list[FaqSuggestionDto])
def suggestions(roles: str = ''):
    role_list = [
        role.strip()
        for role in roles.split(',')
        if role.strip()
    ]

    return get_suggestions(role_list)


@app.get('/faq/recent')
def recent_questions(limit: int = Query(default=20, ge=1, le=100)):
    return get_recent_questions(limit)


@app.get('/faq/analytics')
def analytics() -> dict[str, Any]:
    return get_database_dashboard()