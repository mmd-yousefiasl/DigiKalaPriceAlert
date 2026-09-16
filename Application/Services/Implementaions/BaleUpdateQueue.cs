using Application.Dtos.Bale;
using Application.Services.Interfaces;
using System.Threading.Channels;

namespace Application.Services.Implementaions
{
    public class BaleUpdateQueue : IBaleUpdateQueue
    {
        private readonly Channel<BaleUpdateDto> _channel;

        public BaleUpdateQueue()
        {
            var options = new BoundedChannelOptions(capacity: 1000)
            {
                FullMode = BoundedChannelFullMode.Wait, // apply backpressure if queue fills up
                SingleReader = true,                    // one background worker
                SingleWriter = false                    // many HTTP requests can enqueue
            };

            _channel = Channel.CreateBounded<BaleUpdateDto>(options);
        }

        public ValueTask EnqueueAsync(BaleUpdateDto update, CancellationToken ct = default)
            => _channel.Writer.WriteAsync(update, ct);

        public IAsyncEnumerable<BaleUpdateDto> ReadAllAsync(CancellationToken ct = default)
            => _channel.Reader.ReadAllAsync(ct);
    }
}