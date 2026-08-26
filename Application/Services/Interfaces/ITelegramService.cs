using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ITelegramService
    {
        Task ProcessTelegramUpdateAsync(string body);

        Task<Result> SendTelegramMessage(long chatId, string messageText);

        /// <summary>
        /// Forwards a site-wide notification (created by admin) to the private Telegram channel.
        /// </summary>
        Task<Result> SendSiteNotificationToChannelAsync(string content);

        /// <summary>
        /// Posts an error report to the private Telegram error channel.
        /// Text is HTML-escaped automatically.
        /// </summary>
        Task<Result> SendErrorToChannelAsync(string errorText);

        Task<Result> SendTelegramTestMessage();
    }
}
