using Application.Services.Interfaces;

namespace Web.BackgroundServices
{
    public class PriceMonitorWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PriceMonitorWorker> _logger;

        private const int CycleIntervalMinutes = 15;

        public PriceMonitorWorker(IServiceScopeFactory scopeFactory, ILogger<PriceMonitorWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Price Monitor Worker Started.");

            var cycleInterval = TimeSpan.FromMinutes(CycleIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    _logger.LogInformation("New monitoring cycle started at: {Time}", DateTimeOffset.Now);

                    // Create a scope for the cycle
                    using var scope = _scopeFactory.CreateScope();
                    var updateService = scope.ServiceProvider.GetRequiredService<IProductService>();

                    // Let the service handle EVERYTHING
                    await updateService.ExecuteUpdateCycleAsync(stoppingToken);

                    watch.Stop();

                    var sleepTime = cycleInterval > watch.Elapsed
                        ? cycleInterval - watch.Elapsed
                        : TimeSpan.Zero;

                    _logger.LogInformation(
                        "Cycle finished in {Duration} min. Sleeping for {Sleep} min.",
                        watch.Elapsed.TotalMinutes.ToString("F1"),
                        sleepTime.TotalMinutes.ToString("F1"));

                    if (sleepTime > TimeSpan.Zero)
                        await Task.Delay(sleepTime, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Price Monitor Worker was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in monitoring cycle. Retrying in 5 minutes.");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}