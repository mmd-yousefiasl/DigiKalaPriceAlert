using Application.Dtos.Telegram;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Implementaions
{
    public class TelegramService : ITelegramService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public TelegramService(HttpClient httpClient,
            IUserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }
        #endregion

        public async Task ProcessTelegramUpdateAsync(string body)
        {
            try
            {
                var update = JsonSerializer.Deserialize<TelegramUpdateDto>(
                    body,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (update?.Message == null ||
                    string.IsNullOrWhiteSpace(update.Message.Text))
                {
                    return;
                }

                long chatId = update.Message.Chat.Id;
                string text = update.Message.Text.Trim();

                if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStartAsync(chatId);
                    return;
                }

                if (text.StartsWith("/start ", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStartWithAuthKeyAsync(
                        chatId,
                        text["/start ".Length..].Trim());

                    return;
                }

                await HandleUnknownCommandAsync(chatId);
            }
            catch (Exception ex)
            {
                // Log exception
            }
        }

        private async Task HandleStartAsync(long chatId)
        {
            var message = $"""
        👋 <b>سلام!</b>

        به ربات <b>دیجیتو</b> خوش آمدید. 🎉

        این ربات برای کاربران وب‌سایت دیجیتو طراحی شده است و
        از طریق آن می‌توانید هشدارهای تغییر قیمت محصولات خود را دریافت کنید.

        🔔 <b>با اتصال به ربات:</b>
        • هشدارهای قیمت را مستقیماً در تلگرام دریافت می‌کنید.
        • از تغییرات مهم قیمت محصولات مطلع می‌شوید.

        🔗 <b>برای شروع و مدیریت هشدارها:</b>
        <a href="{SiteTools.SiteUrl}">ورود به وب‌سایت دیجیتو</a>

        ❤️ از همراهی شما ممنونیم.
        """;

            await SendTelegramMessage(chatId, message);
        }

        private async Task HandleStartWithAuthKeyAsync(long chatId, string authKey)
        {
            var result =
                await _userService.ClientSideAuthenticateBaleUserAsync(
                    authKey,
                    chatId);

            if (result.IsSuccess)
            {
                string authSuccessMessage = $"""
    ✅ <b>حساب تلگرام شما با موفقیت متصل شد!</b>

    از این پس هشدارهای قیمتی دیجیتو
    از طریق همین ربات برای شما ارسال خواهد شد.

    🔗 <a href="{SiteTools.SiteUrl}">مدیریت هشدارها در وب‌سایت دیجیتو</a>
    """;
                await SendTelegramMessage(chatId, authSuccessMessage);

                return;
            }

            string authFailedMessage = $"""
        ❌ <b>احراز هویت ناموفق بود.</b>

        {result.Message}

        لطفاً از داخل وب‌سایت دیجیتو
        دوباره فرآیند اتصال تلگرام را انجام دهید:

        <a href="{SiteTools.SiteUrl}">ورود به وب‌سایت دیجیتو</a>
        """;
            await SendTelegramMessage(chatId, authFailedMessage);
        }

        private async Task HandleUnknownCommandAsync(long chatId)
        {
            string message = $"""
        🤖 متوجه دستور شما نشدم.

        این ربات برای ارسال هشدارهای قیمتی دیجیتو است.

        برای مدیریت حساب و هشدارهای خود وارد سایت شوید:

        <a href="{SiteTools.SiteUrl}">ورود به وب‌سایت دیجیتو</a>
        """;

            await SendTelegramMessage(chatId, message);
        }


        public Task<Result> SendTelegramMessage(long chatId, string messageText)
        {
            return PostMessageAsync(chatId, messageText);
        }

        public Task<Result> SendSiteNotificationToChannelAsync(string content)
        {
            if (SiteTools.TelegramNotificationChannelId == 0 ||
                string.IsNullOrWhiteSpace(content))
            {
                return Task.FromResult(Result.Failure(Messages.OperationFailedError));
            }

            string message = $"""
    📢 <b>اعلان جدید از دیجیتو</b>

    {content}
    """;

            return PostMessageAsync(SiteTools.AdminTelegramWorkerSendUrl,
                SiteTools.AdminTelegramWorkerKey,
                SiteTools.TelegramNotificationChannelId,
                message);
        }

        public async Task<Result> SendErrorToChannelAsync(string errorText)
        {
            if (SiteTools.TelegramErrorChannelId == 0 ||
                string.IsNullOrWhiteSpace(errorText))
            {
                return Result.Failure(Messages.OperationFailedError);
            }

            // Telegram rejects messages whose HTML tags don't balance — stack traces
            // are full of angle brackets, so escape everything and render as code.
            var escaped = HtmlEscape(errorText);

            if (escaped.Length > MaxTelegramErrorLength)
            {
                escaped = escaped[..MaxTelegramErrorLength] + "\n…";
            }

            return await PostMessageAsync(
                SiteTools.AdminTelegramWorkerSendUrl,
                SiteTools.AdminTelegramWorkerKey,
                SiteTools.TelegramErrorChannelId,
                $"<pre>{escaped}</pre>");
        }

        private const int MaxTelegramErrorLength = 3500;

        private static string HtmlEscape(string text)
        {
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;");
        }

        private async Task<Result> PostMessageAsync(long chatId, string messageText)
        {
            var payload = new
            {
                chat_id = chatId,
                text = messageText,
                parse_mode = "HTML",

                // Telegram expands the product URL into a huge photo preview —
                // keep alert messages compact.
                link_preview_options = new { is_disabled = true }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                SiteTools.TelegramWorkerSendUrl);

            request.Headers.Add(
                "Authorization",
                $"Bearer {SiteTools.TelegramWorkerKey}");

            request.Content = JsonContent.Create(payload);

            try
            {
                await _httpClient.SendAsync(request);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Messages.OperationFailedError);
            }
        }

        private async Task<Result> PostMessageAsync(string TelegramWorkerSendUrl, string TelegramWorkerKey, long chatId, string messageText)
        {
            var payload = new
            {
                chat_id = chatId,
                text = messageText,
                parse_mode = "HTML",

                // Telegram expands the product URL into a huge photo preview —
                // keep alert messages compact.
                link_preview_options = new { is_disabled = true }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                TelegramWorkerSendUrl);

            request.Headers.Add(
                "Authorization",
                $"Bearer {TelegramWorkerKey}");

            request.Content = JsonContent.Create(payload);

            try
            {
                await _httpClient.SendAsync(request);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Messages.OperationFailedError);
            }
        }

        public async Task<Result> SendTelegramTestMessage()
        {
            var chatIdResult = await _userService.ClientSideGetCurrentUserChatId();

            if (chatIdResult.IsFailure)
                return Result.Failure(chatIdResult.Message!);

            string message = $"""
    <b>🎉 اتصال تلگرام با موفقیت انجام شد!</b>

    ✅ حساب تلگرام شما با موفقیت به حساب کاربری‌تان متصل شد.

    🔔 <b>از این پس چه اتفاقی می‌افتد؟</b>
    • هشدارهای تغییر قیمت را دریافت می‌کنید.
    • اعلان‌های مهم مستقیماً در تلگرام برای شما ارسال می‌شوند.

    💡 <i>این پیام فقط برای تست اتصال ارسال شده است.</i>

    ❤️ ممنون که از <a href="{SiteTools.SiteUrl}">دیجیتو</a> استفاده می‌کنید.
    """;

            return await SendTelegramMessage(chatIdResult.Value, message);
        }
    }
}
