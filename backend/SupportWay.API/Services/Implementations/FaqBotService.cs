using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SupportWay.API.DTOs;
using SupportWay.API.Services.Interface;

namespace SupportWay.API.Services.Implementations;

public class FaqBotService : IFaqBotService
{
    private static readonly Regex TokenRegex = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _httpClient;
    private readonly ILogger<FaqBotService> _logger;

    private readonly List<FaqEntry> _fallbackEntries = new()
    {
        new FaqEntry(
            Category: "Акаунт",
            Questions: new[] { "як зареєструватися", "як увійти", "логін", "реєстрація", "акаунт", "профіль", "пароль" },
            Answer: "Щоб користуватися SupportWay, потрібно зареєструватися або увійти в акаунт. Після входу можна заповнити профіль, додати фото, переглядати пости, запити, карту та чати.",
            Actions: new[] { new FaqQuickActionDto { Label = "На головну", Route = "/home" } }
        ),
        new FaqEntry(
            Category: "Верифікація",
            Questions: new[] { "як пройти верифікацію", "стати волонтером", "стати військовим", "роль", "галочка", "бейдж", "заявка" },
            Answer: "Після реєстрації користувач має базову роль User. Щоб отримати статус волонтера або військового, потрібно подати заявку у профілі. Після схвалення адміністратором роль оновиться, а біля username зʼявиться відповідна іконка.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити профіль", Route = "/profile/me" } }
        ),
        new FaqEntry(
            Category: "Запити допомоги",
            Questions: new[] { "створити запит", "запит допомоги", "help request", "потреби", "допомога", "збір" },
            Answer: "Запит допомоги описує конкретну потребу: категорію, речі, суму збору, локацію та деталі. Такі запити видно у вкладці Запити та на карті, якщо автор додав координати.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити запити", Route = "/requests" }, new FaqQuickActionDto { Label = "Відкрити карту", Route = "/map" } }
        ),
        new FaqEntry(
            Category: "Донати",
            Questions: new[] { "як задонатити", "донат", "платіж", "картка", "iban", "переказ", "збір коштів" },
            Answer: "Донат доступний у запитах допомоги та на карті. Якщо автор додав реквізити, система показує картку, IBAN, посилання на оплату або додаткові інструкції.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити карту", Route = "/map" }, new FaqQuickActionDto { Label = "Відкрити запити", Route = "/requests" } }
        ),
        new FaqEntry(
            Category: "Карта",
            Questions: new[] { "карта", "локація", "геолокація", "запити поруч", "маркер", "відстань", "координати" },
            Answer: "Карта допомагає знайти запити допомоги за місцем. Маркери показують запити, а панель деталей дозволяє переглянути потреби та перейти до донату.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити карту", Route = "/map" } }
        ),
        new FaqEntry(
            Category: "Публікації",
            Questions: new[] { "пост", "публікація", "лайк", "коментар", "стрічка", "поділитися", "репост" },
            Answer: "У вкладці Публікації можна переглядати дописи, ставити лайки, коментувати та ділитися постами в чат. Запити допомоги відокремлені від звичайних постів, щоб інтерфейс не змішував типи контенту.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити публікації", Route = "/posts" } }
        ),
        new FaqEntry(
            Category: "Чати",
            Questions: new[] { "чат", "повідомлення", "репост в чат", "поділитися в чат", "видалити повідомлення", "прочитано", "signalr", "mongodb" },
            Answer: "Чати працюють у реальному часі через SignalR. Повідомлення зберігаються в MongoDB, можна надсилати текст, бачити статус прочитання, видаляти власні повідомлення та ділитися постами або запитами.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити чати", Route = "/chat" } }
        ),
        new FaqEntry(
            Category: "Адмін-панель",
            Questions: new[] { "адмін", "адміністратор", "панель", "схвалити заявку", "відхилити заявку", "верифікація користувачів" },
            Answer: "Адмін-панель доступна тільки користувачам із роллю Admin. Там адміністратор переглядає заявки на верифікацію, відкриває профілі, додає коментар і схвалює або відхиляє заявки.",
            Actions: new[] { new FaqQuickActionDto { Label = "Відкрити адмін-панель", Route = "/admin" } },
            Roles: new[] { "admin" }
        )
    };

    public FaqBotService(HttpClient httpClient, ILogger<FaqBotService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FaqBotResponseDto> AskAsync(string question, IEnumerable<string> userRoles, CancellationToken cancellationToken = default)
    {
        var roles = userRoles?.ToList() ?? new List<string>();

        try
        {
            var payload = new PythonFaqRequest(question ?? string.Empty, roles);
            var response = await _httpClient.PostAsJsonAsync("faq", payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FaqBotResponseDto>(cancellationToken: cancellationToken);
                if (result is not null)
                {
                    return result;
                }
            }

            _logger.LogWarning("Python FAQ service returned {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python FAQ service is unavailable. Fallback FAQ mode enabled.");
        }

        return AskFallback(question, roles);
    }

    public async Task<IReadOnlyList<FaqSuggestionDto>> GetSuggestionsAsync(IEnumerable<string> userRoles, CancellationToken cancellationToken = default)
    {
        var roles = userRoles?.ToList() ?? new List<string>();

        try
        {
            var query = roles.Count > 0
                ? $"faq/suggestions?roles={Uri.EscapeDataString(string.Join(',', roles))}"
                : "faq/suggestions";

            var result = await _httpClient.GetFromJsonAsync<List<FaqSuggestionDto>>(query, cancellationToken);
            if (result is not null)
            {
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python FAQ suggestions are unavailable. Fallback suggestions enabled.");
        }

        return GetFallbackSuggestions(roles);
    }

    private FaqBotResponseDto AskFallback(string question, IEnumerable<string> userRoles)
    {
        var normalizedQuestion = question?.Trim() ?? string.Empty;
        var roleSet = userRoles.Select(r => r.ToLowerInvariant()).ToHashSet();
        var availableEntries = GetAvailableFallbackEntries(roleSet).ToList();

        if (string.IsNullOrWhiteSpace(normalizedQuestion))
        {
            return new FaqBotResponseDto
            {
                Category = "Підказка",
                Confidence = 0,
                Answer = "Напишіть питання про реєстрацію, верифікацію, запити допомоги, донати, карту, публікації або чати.",
                Suggestions = availableEntries.Take(5).Select(e => e.MainQuestion).ToList()
            };
        }

        var questionTokens = Tokenize(normalizedQuestion).ToList();
        var best = availableEntries
            .Select(entry => new { Entry = entry, Score = Score(entry, normalizedQuestion, questionTokens) })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (best is null || best.Score <= 0)
        {
            return new FaqBotResponseDto
            {
                Category = "Не знайдено",
                Confidence = 0,
                Answer = "Я не знайшов точної відповіді, але можу допомогти з реєстрацією, верифікацією, запитами допомоги, донатами, картою, постами або чатами. Спробуйте сформулювати питання трохи інакше.",
                Suggestions = availableEntries.Take(5).Select(e => e.MainQuestion).ToList()
            };
        }

        var confidence = Math.Min(0.98, Math.Round(best.Score / Math.Max(4, questionTokens.Count + 3), 2));
        var suggestions = availableEntries
            .Where(e => e.Category != best.Entry.Category)
            .Take(4)
            .Select(e => e.MainQuestion)
            .ToList();

        return new FaqBotResponseDto
        {
            Category = best.Entry.Category,
            Confidence = confidence,
            Answer = best.Entry.Answer,
            Actions = best.Entry.Actions.ToList(),
            Suggestions = suggestions
        };
    }

    private IReadOnlyList<FaqSuggestionDto> GetFallbackSuggestions(IEnumerable<string> userRoles)
    {
        var roleSet = userRoles.Select(r => r.ToLowerInvariant()).ToHashSet();
        return GetAvailableFallbackEntries(roleSet)
            .Select(e => new FaqSuggestionDto { Text = e.MainQuestion, Category = e.Category })
            .ToList();
    }

    private IEnumerable<FaqEntry> GetAvailableFallbackEntries(HashSet<string> roleSet)
    {
        return _fallbackEntries.Where(e => e.Roles.Length == 0 || e.Roles.Any(roleSet.Contains));
    }

    private static double Score(FaqEntry entry, string question, IReadOnlyList<string> tokens)
    {
        var score = 0.0;
        var lowerQuestion = question.ToLowerInvariant();

        foreach (var phrase in entry.Questions)
        {
            var lowerPhrase = phrase.ToLowerInvariant();

            if (lowerQuestion.Contains(lowerPhrase))
            {
                score += 4;
            }

            var phraseTokens = Tokenize(lowerPhrase).ToHashSet();
            score += tokens.Count(token => phraseTokens.Contains(token)) * 1.4;
        }

        score += Tokenize(entry.Category).Count(tokens.Contains) * 1.8;
        return score;
    }

    private static IEnumerable<string> Tokenize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (Match match in TokenRegex.Matches(value.ToLowerInvariant()))
        {
            var token = match.Value.Trim();
            if (token.Length >= 2)
            {
                yield return token;
            }
        }
    }

    private sealed record PythonFaqRequest(string Question, List<string> Roles);

    private sealed class FaqEntry
    {
        public FaqEntry(
            string Category,
            string[] Questions,
            string Answer,
            FaqQuickActionDto[] Actions,
            string[]? Roles = null)
        {
            this.Category = Category;
            this.Questions = Questions;
            this.Answer = Answer;
            this.Actions = Actions;
            this.Roles = Roles ?? Array.Empty<string>();
        }

        public string Category { get; }
        public string[] Questions { get; }
        public string Answer { get; }
        public FaqQuickActionDto[] Actions { get; }
        public string[] Roles { get; }
        public string MainQuestion => Questions.FirstOrDefault() ?? Category;
    }
}
