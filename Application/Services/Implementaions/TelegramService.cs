using Application.Dtos.Telegram;
using Application.Services.Interfaces;
using Domain.Statics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Services.Implementaions
{
    public class TelegramService : ITelegramService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        #endregion

        #region Ctor
        public TelegramService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

                if (update?.Message == null || string.IsNullOrWhiteSpace(update.Message.Text))
                {
                    return;
                }

                long chatId = update.Message.Chat.Id;
                string text = update.Message.Text.Trim();

                // ==========================================
                // /start
                // ==========================================

                if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await SendTelegramMessage(
                        chatId,
                        """
            👋 سلام!

            به ربات دیجی‌پرایس‌الرت خوش آمدید.

            این ربات برای کاربران وب‌سایت دیجی‌پرایس‌الرت طراحی شده و
            از طریق آن می‌توانید هشدارهای قیمتی محصولات خود را دریافت کنید.

            🔗 برای استفاده از خدمات و مدیریت هشدارها وارد سایت شوید:

            https://digipricealert.ir
            """);

                    return;
                }

                // ==========================================
                // /start AUTH_KEY
                // ==========================================

                if (text.StartsWith("/start ", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = text.Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

                    string authKey = parts.Length > 1
                        ? parts[1].Trim()
                        : string.Empty;

                    if (string.IsNullOrWhiteSpace(authKey))
                    {
                        await SendTelegramMessage(
                            chatId,
                            "❌ کلید احراز هویت ارسال نشده است.\n\nلطفاً از داخل وب‌سایت دیجی‌پرایس‌الرت اقدام کنید.");

                        return;
                    }

                    // ==========================================
                    // TODO:
                    // Validate authKey against your database
                    // ==========================================

                    bool authenticated = await AuthenticateTelegramUserAsync(
                        authKey,
                        chatId);

                    if (authenticated)
                    {
                        await SendTelegramMessage(
                            chatId,
                            """
                ✅ حساب تلگرام شما با موفقیت متصل شد!

                از این پس هشدارهای قیمتی دیجی‌پرایس‌الرت
                از طریق همین ربات برای شما ارسال خواهد شد.
                """);
                    }
                    else
                    {
                        await SendTelegramMessage(
                            chatId,
                            """
                ❌ احراز هویت ناموفق بود.

                کلید اتصال معتبر نیست یا منقضی شده است.

                لطفاً از داخل وب‌سایت دیجی‌پرایس‌الرت
                دوباره فرآیند اتصال تلگرام را انجام دهید:

                https://digipricealert.ir
                """);
                    }

                    return;
                }

                // ==========================================
                // Everything else
                // ==========================================

                await SendTelegramMessage(
                    chatId,
                    """
        🤖 متوجه دستور شما نشدم.

        این ربات برای ارسال هشدارهای قیمتی دیجی‌پرایس‌الرت است.

        برای مدیریت حساب و هشدارهای خود وارد سایت شوید:

        https://digipricealert.ir
        """);
            }
            catch (Exception ex)
            {
                // Log exception
            }
        }

        #region ProcessTelegramUpdateAsync Private Methods
        private async Task<bool> AuthenticateTelegramUserAsync(string authKey, long chatId)
        {
            // TODO:
            // Check authKey in your database.
            //
            // Example:
            //
            // var user = await _dbContext.Users
            //     .FirstOrDefaultAsync(x => x.TelegramAuthKey == authKey);
            //
            // if (user == null)
            //     return false;
            //
            // user.TelegramChatId = chatId;
            // user.TelegramConnected = true;
            //
            // await _dbContext.SaveChangesAsync();
            //
            // return true;

            return false;
        }

        private async Task SendTelegramMessage(long chatId, string messageText)
        {
            var payload = new
            {
                chat_id = chatId,
                text = messageText,
                parse_mode = "HTML"
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
            }
            catch (Exception ex)
            {
                // TODO: Log exception
            }
        }
        #endregion
    }
}
