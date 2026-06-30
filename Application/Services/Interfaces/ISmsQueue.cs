using Application.Dtos.SmsConsumer;

namespace Application.Services.Interfaces
{
    public interface ISmsQueue
    {
        ValueTask QueueSmsAsync(SmsQueueItem message);
        ValueTask<SmsQueueItem> DequeueSmsAsync(CancellationToken cancellationToken);
    }
}
