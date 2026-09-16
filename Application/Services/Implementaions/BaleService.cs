using Application.Dtos.Bale;
using Application.Services.Interfaces;
using Domain.Statics;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Application.Services.Implementaions
{
    public class BaleService : IBaleService
    {
        #region Fields
        private readonly HttpClient _http;
        private readonly ILogger<BaleService> _logger;
        #endregion

        #region Ctor
        public BaleService(HttpClient http, ILogger<BaleService> logger)
        {
            _http = http;
            _logger = logger;
        }
        #endregion

        public async Task SendMessageAsync(long chatId, string text, string? parseMode = null, object? replyMarkup = null, CancellationToken ct = default)
        {
            var url = $"{SiteTools.BaleBaseUrl}{SiteTools.BaleBotToken}/sendMessage";
            var payload = new SendMessageRequestDto
            {
                ChatId = chatId,
                Text = text,
                ParseMode = parseMode,
                ReplyMarkup = replyMarkup
            };

            var response = await _http.PostAsJsonAsync(url, payload, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Bale sendMessage failed: {Status} {Body}", response.StatusCode, body);
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

            _logger.LogInformation("Bale msg from {ChatId}: {Text}", chatId, text);

            // Simple command router
            switch (text)
            {
                case "/start":
                    await SendMessageAsync(chatId,
                        $"سلام {msg.From?.FirstName ?? ""}! 👋\nخوش آمدید.",
                        ct: ct);
                    break;

                case "/help":
                    await SendMessageAsync(chatId,
                        "دستورات موجود:\n/start - شروع\n/help - راهنما",
                        ct: ct);
                    break;

                default:
                    await SendMessageAsync(chatId, $"دریافت شد: {text}", ct: ct);
                    break;
            }
        }

        private async Task HandleCallbackAsync(BaleCallbackQueryDto cb, CancellationToken ct)
        {
            _logger.LogInformation("Bale callback from {UserId}: {Data}", cb.From?.Id, cb.Data);

            if (cb.From is not null)
            {
                await SendMessageAsync(cb.From.Id, $"شما انتخاب کردید: {cb.Data}", ct: ct);
            }

            // Acknowledge the callback (optional but recommended)
            var ackUrl = $"{SiteTools.BaleBaseUrl}{SiteTools.BaleBotToken}/answerCallbackQuery";
            await _http.PostAsJsonAsync(ackUrl, new { callback_query_id = cb.Id }, ct);
        }
    }
}