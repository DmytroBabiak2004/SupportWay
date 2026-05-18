using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SupportWay.API.Services.Implementations
{
    public class MonobankJarService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MonobankJarService> _logger;

        public MonobankJarService(HttpClient http, ILogger<MonobankJarService> logger)
        {
            _http = http;
            _logger = logger;
        }
        public async Task<decimal?> GetJarBalanceAsync(string jarId, CancellationToken ct = default)
        {
            try
            {
                var url = $"https://api.monobank.ua/bank/jar/{jarId}";
                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Monobank jar API returned {StatusCode} for jar {JarId}",
                        (int)response.StatusCode, jarId);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("amount", out var amountProp))
                {
                    var kopecks = amountProp.GetInt64();
                    return Math.Round(kopecks / 100m, 2);
                }

                _logger.LogWarning(
                    "Monobank jar API response missing 'amount' for jar {JarId}: {Json}",
                    jarId, json);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to fetch Monobank jar balance for jar {JarId}", jarId);
                return null;
            }
        }
        public async Task<MonobankJarInfo?> GetJarInfoAsync(string jarId, CancellationToken ct = default)
        {
            try
            {
                var url = $"https://api.monobank.ua/bank/jar/{jarId}";
                var response = await _http.GetAsync(url, ct);

                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return new MonobankJarInfo
                {
                    Title = root.TryGetProperty("title", out var t) ? t.GetString() : null,
                    Description = root.TryGetProperty("description", out var d) ? d.GetString() : null,
                    AmountUah = root.TryGetProperty("amount", out var a)
                                    ? Math.Round(a.GetInt64() / 100m, 2) : 0m,
                    GoalUah = root.TryGetProperty("goal", out var g)
                                    ? Math.Round(g.GetInt64() / 100m, 2) : 0m,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch jar info for {JarId}", jarId);
                return null;
            }
        }
    }

    public sealed class MonobankJarInfo
    {
        public string? Title { get; init; }
        public string? Description { get; init; }
        public decimal AmountUah { get; init; }
        public decimal GoalUah { get; init; }
    }
}