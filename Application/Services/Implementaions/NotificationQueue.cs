using Application.Dtos.NotificationQueues;
using Application.Services.Interfaces;
using System.Threading.Channels;

namespace Application.Services.Implementaions
{
    public class NotificationQueue : INotificationQueue
    {
        #region Fields
        private readonly Channel<NotificationMessage> _channel;
        #endregion

        #region Ctor
        public NotificationQueue()
        {
            _channel = Channel.CreateUnbounded<NotificationMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }
        #endregion

        public ValueTask QueueNotificationAsync(NotificationMessage notification)
        {
            return _channel.Writer.WriteAsync(notification);
        }

        public ValueTask<NotificationMessage> DequeueNotificationAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
