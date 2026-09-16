using Application.Dtos.Bale;

namespace Application.Services.Interfaces
{
    public interface IBaleService
    {
        Task SendMessageAsync(long chatId, string text, string? parseMode = null, object? replyMarkup = null, CancellationToken ct = default);
        Task HandleUpdateAsync(BaleUpdateDto update, CancellationToken ct = default);
    }
}
