using Application.Dtos.NotificationQueues;

namespace Application.Services.Interfaces
{
    public interface ISmsQueue
    {
        ValueTask QueueSmsAsync(SmsMessage message);
        ValueTask<SmsMessage> DequeueSmsAsync(CancellationToken cancellationToken);
    }
}
