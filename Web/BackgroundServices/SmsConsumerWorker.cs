using Application.Services.Interfaces;

namespace Web.BackgroundServices
{
    public class SmsConsumerWorker : BackgroundService
    {
        private readonly ISmsQueue _queue;
        private readonly ILogger<SmsConsumerWorker> _logger;

        public SmsConsumerWorker(
            ISmsQueue queue,
            ILogger<SmsConsumerWorker> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Consumer Worker started...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notification = await _queue.DequeueSmsAsync(stoppingToken);

                    //Send SmsHere

                    await Task.Delay(50, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending queued notification.");
                }
            }
        }
    }
}
