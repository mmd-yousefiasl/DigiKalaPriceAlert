using Application.Dtos.Bale;

namespace Application.Services.Interfaces
{
    public interface IBaleUpdateQueue
    {
        ValueTask EnqueueAsync(BaleUpdateDto update, CancellationToken ct = default);
        IAsyncEnumerable<BaleUpdateDto> ReadAllAsync(CancellationToken ct = default);
    }
}
