using Application.Dtos.Bale;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Application.Services.Implementaions
{
    public class BaleService : IBaleService
    {
        #region Fields
        private readonly HttpClient _http;
        private readonly ILogger<BaleService> _logger;
        private readonly IUserService _userService;

        private delegate Task CommandHandler(long chatId, string args, CancellationToken ct);

        // Command → handler map. Add new commands here; handlers live in #region Commands.
        // Keys are matched case-insensitively.
        private readonly Dictionary<string, CommandHandler> _commands;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        #endregion

        #region Ctor
        public BaleService(
            HttpClient http,
            ILogger<BaleService> logger,
            IUserService userService)
        {
            _http = http;
            _logger = logger;
            _userService = userService;

            _commands = new(StringComparer.OrdinalIgnoreCase)
            {
                ["/start"] = HandleStartAsync,

                // ---- Add future commands here, e.g. ----
                // ["/help"]     = HandleHelpAsync,
                // ["/settings"] = HandleSettingsAsync,
            };
        }
        #endregion

        #region Update pipeline

        /// <summary>
        /// Deserializes a raw Bale webhook body and routes it — same role as
        /// TelegramService.ProcessTelegramUpdateAsync.
        /// </summary>
        public async Task ProcessUpdateAsync(string body, CancellationToken ct = default)
        {
            try
            {
                var update = JsonSerializer.Deserialize<BaleUpdateDto>(body, JsonOptions);
                if (update is null)
                    return;

                await HandleUpdateAsync(update, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Bale update");
            }
        }

        public async Task HandleUpdateAsync(BaleUpdateDto update, CancellationToken ct = default)
        {
            // Callback button pressed
            if (update.CallbackQuery is not null)
            {
                await HandleCallbackAsync(update.CallbackQuery, ct);
                return;
            }

            if (update.Message is null)
                return;

            var msg = update.Message;
            var chatId = msg.Chat.Id;
            var text = msg.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return;

            _logger.LogInformation("Bale msg from {ChatId}: {Text}", chatId, text);

            // ---- Command routing: "/cmd arg1 arg2" → command="/cmd", args="arg1 arg2" ----
            if (text.StartsWith('/'))
            {
                var spaceIndex = text.IndexOf(' ');
                var command = spaceIndex < 0 ? text : text[..spaceIndex];
                var args = spaceIndex < 0 ? string.Empty : text[(spaceIndex + 1)..].Trim();

                // Strip "@botname" suffix ("/start@MyBot" → "/start")
                var atIndex = command.IndexOf('@');
                if (atIndex > 0)
                    command = command[..atIndex];

                if (_commands.TryGetValue(command, out var handler))
                {
                    await handler(chatId, args, ct);
                    return;
                }
            }

            await HandleUnknownCommandAsync(chatId, ct);
        }

        private async Task HandleCallbackAsync(BaleCallbackQueryDto cb, CancellationToken ct)
        {
            _logger.LogInformation("Bale callback from {UserId}: {Data}", cb.From?.Id, cb.Data);

            if (cb.From is not null)
            {
                await SendMessageAsync(cb.From.Id, $"شما انتخاب کردید: {cb.Data}", ct: ct);
            }

            // Acknowledge the callback (optional but recommended)
            var ackUrl = $"{SiteTools.BaleBaseAPIUrl}{SiteTools.BaleBotToken}/answerCallbackQuery";
            await _http.PostAsJsonAsync(ackUrl, new { callback_query_id = cb.Id }, ct);
        }

        #endregion

        #region Commands

        // /start              → welcome message
        // /start <authKey>    → link account
        private async Task HandleStartAsync(long chatId, string args, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                await SendWelcomeAsync(chatId, ct);
                return;
            }

            await HandleStartWithAuthKeyAsync(chatId, args, ct);
        }

        private async Task SendWelcomeAsync(long chatId, CancellationToken ct)
        {
            var message = $"""
👋 *سلام!*

به ربات *دیجیتو* خوش آمدید. 🎉

این ربات برای کاربران وب‌سایت دیجیتو طراحی شده است و
از طریق آن می‌توانید هشدارهای تغییر قیمت محصولات خود را دریافت کنید.

🔔 *با اتصال به ربات:*
• هشدارهای قیمت را مستقیماً در بله دریافت می‌کنید.
• از تغییرات مهم قیمت محصولات مطلع می‌شوید.

🔗 *برای شروع و مدیریت هشدارها:*
[ورود به وب‌سایت دیجیتو]({SiteTools.SiteUrl})

❤️ از همراهی شما ممنونیم.
""";
            await SendMessageAsync(chatId, message, ct: ct);
        }

        private async Task HandleStartWithAuthKeyAsync(long chatId, string authKey, CancellationToken ct)
        {
            var result = await _userService.ClientSideAuthenticateBaleUserAsync(authKey, chatId);

            string message;

            if (result.IsSuccess)
            {
                message = $"""
✅ *حساب بله شما با موفقیت متصل شد!*

از این پس هشدارهای قیمتی دیجیتو
از طریق همین ربات برای شما ارسال خواهد شد.

🔗 [مدیریت هشدارها در وب‌سایت دیجیتو]({SiteTools.SiteUrl}user/products/add)
""";
            }
            else
            {
                message = $"""
❌ *احراز هویت ناموفق بود.*

{EscapeMarkdown(result.Message)}

لطفاً از داخل وب‌سایت دیجیتو
دوباره فرآیند اتصال بله را انجام دهید:

[ورود به وب‌سایت دیجیتو]({SiteTools.SiteUrl}user/profile/edit)
""";
            }

            await SendMessageAsync(chatId, message, ct: ct);
        }

        private async Task HandleUnknownCommandAsync(long chatId, CancellationToken ct)
        {
            string message = $"""
🤖 متوجه دستور شما نشدم.

این ربات برای ارسال هشدارهای قیمتی دیجیتو است.

برای مدیریت حساب و هشدارهای خود وارد سایت شوید:

[ورود به وب‌سایت دیجیتو]({SiteTools.SiteUrl}user)
""";
            await SendMessageAsync(chatId, message, ct: ct);
        }

        #endregion

        #region Outgoing

        public async Task<Result> SendMessageAsync(
            long chatId,
            string text,
            object? replyMarkup = null,
            CancellationToken ct = default)
        {
            return await PostMessageAsync(chatId, text, replyMarkup, ct);
        }

        public async Task<Result> SendBaleTestMessage(CancellationToken ct = default)
        {
            var chatIdResult = await _userService.ClientSideGetCurrentUserChatId();

            if (chatIdResult.IsFailure)
                return Result.Failure(chatIdResult.Message!);

            string message = $"""
🎉 *اتصال بله با موفقیت انجام شد!*

✅ حساب بله شما با موفقیت به حساب کاربری‌تان متصل شد.

🔔 *از این پس چه اتفاقی می‌افتد؟*
• هشدارهای تغییر قیمت را دریافت می‌کنید.
• اعلان‌های مهم مستقیماً در بله برای شما ارسال می‌شوند.

💡 _این پیام فقط برای تست اتصال ارسال شده است._

❤️ ممنون که از [دیجیتو]({SiteTools.SiteUrl}) استفاده می‌کنید.
""";

            return await SendMessageAsync(chatIdResult.Value, message, ct: ct);
        }

        #endregion

        #region Private helpers

        private async Task<Result> PostMessageAsync(
            long chatId,
            string text,
            object? replyMarkup,
            CancellationToken ct)
        {
            var url = $"{SiteTools.BaleBaseAPIUrl}{SiteTools.BaleBotToken}/sendMessage";

            // IMPORTANT: Bale ignores `parse_mode` and always renders Markdown.
            // Do NOT send parse_mode. Format messages with *bold*, _italic_, [text](url).
            var payload = new
            {
                chat_id = chatId,
                text = text,
                reply_markup = replyMarkup,
            };

            try
            {
                var response = await _http.PostAsJsonAsync(url, payload, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError(
                        "Bale sendMessage failed: {Status} {Body}",
                        response.StatusCode, body);

                    return Result.Failure(Messages.OperationFailedError);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bale sendMessage exception");
                return Result.Failure(Messages.OperationFailedError);
            }
        }

        // Escapes Markdown metacharacters so dynamic text (e.g. an error message
        // from IUserService) can't break the surrounding formatting.
        private static string EscapeMarkdown(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                if (c is '\\' or '*' or '_' or '`' or '[' or ']')
                    sb.Append('\\');

                sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion
    }
}