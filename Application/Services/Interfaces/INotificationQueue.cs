using Application.Dtos.NotificationQueues;

namespace Application.Services.Interfaces
{
    public interface INotificationQueue
    {
        ValueTask QueueNotificationAsync(NotificationMessage notification);
        ValueTask<NotificationMessage> DequeueNotificationAsync(CancellationToken cancellationToken);
    }
}
