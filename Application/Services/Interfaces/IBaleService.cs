using Application.Dtos.Bale;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IBaleService
    {
        Task ProcessUpdateAsync(string body, CancellationToken ct = default);
        Task HandleUpdateAsync(BaleUpdateDto update, CancellationToken ct = default);

        Task<Result> SendMessageAsync(
            long chatId,
            string text,
            object? replyMarkup = null,
            CancellationToken ct = default);

        Task<Result> SendBaleTestMessage(CancellationToken ct = default);
    }
}
