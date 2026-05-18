using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SupportWay.Data.Context;

namespace SupportWay.API.Services.Implementations
{
    /// <summary>
    /// Фоновий сервіс — кожні N хвилин читає баланс банок Monobank
    /// і оновлює CollectedAmount у всіх активних HelpRequest.
    /// Повністю безкоштовно, без токена, через публічне API.
    /// </summary>
    public class JarSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MonobankJarService _jarService;
        private readonly IConfiguration _config;
        private readonly ILogger<JarSyncBackgroundService> _logger;

        public JarSyncBackgroundService(
            IServiceScopeFactory scopeFactory,
            MonobankJarService jarService,
            IConfiguration config,
            ILogger<JarSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _jarService = jarService;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalMinutes = _config.GetValue<int>("Monobank:JarSyncIntervalMinutes", 5);
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            _logger.LogInformation(
                "JarSyncBackgroundService started. Interval: {Interval} min", intervalMinutes);

            // Перша синхронізація одразу після старту (через 10 сек)
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SyncAllJarsAsync(stoppingToken);
                await Task.Delay(interval, stoppingToken);
            }
        }

        private async Task SyncAllJarsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupportWayContext>();

            try
            {
                // Отримати всі активні реквести з налаштованою банкою
                var defaultJarId = _config["Monobank:DefaultJarId"];

                var requests = await db.HelpRequests
                    .Where(h => h.IsActive)
                    .ToListAsync(ct);

                // Групуємо за jar ID щоб не дублювати запити до API
                var jarGroups = requests
                    .Select(r => new
                    {
                        Request = r,
                        JarId = !string.IsNullOrWhiteSpace(r.MonobankJarId)
                                    ? r.MonobankJarId
                                    : defaultJarId
                    })
                    .Where(x => !string.IsNullOrWhiteSpace(x.JarId))
                    .GroupBy(x => x.JarId);

                var updatedCount = 0;

                foreach (var group in jarGroups)
                {
                    var jarId = group.Key!;
                    var balance = await _jarService.GetJarBalanceAsync(jarId, ct);

                    if (balance is null)
                    {
                        _logger.LogWarning(
                            "Could not read jar balance for {JarId}, skipping", jarId);
                        continue;
                    }

                    foreach (var item in group)
                    {
                        // Оновлюємо тільки якщо сума реально змінилась
                        if (item.Request.CollectedAmount == balance.Value) continue;

                        _logger.LogInformation(
                            "JarSync: HelpRequest {Id} — {Old} → {New} ₴",
                            item.Request.Id, item.Request.CollectedAmount, balance.Value);

                        item.Request.CollectedAmount = balance.Value;

                        // Автоматично закрити збір якщо ціль досягнута
                        if (item.Request.TargetAmount > 0 &&
                            item.Request.CollectedAmount >= item.Request.TargetAmount)
                        {
                            item.Request.IsActive = false;
                            _logger.LogInformation(
                                "JarSync: HelpRequest {Id} goal reached, closing", item.Request.Id);
                        }

                        updatedCount++;
                    }

                    // Throttle — не флудимо API (публічний ліміт ~1 запит/сек)
                    await Task.Delay(1200, ct);
                }

                if (updatedCount > 0)
                    await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "JarSync completed. Updated {Count} requests", updatedCount);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "JarSync failed");
            }
        }
    }
}