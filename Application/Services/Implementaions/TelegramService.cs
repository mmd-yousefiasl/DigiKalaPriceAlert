using Application.Dtos.TelegramDto;
using Application.Services.Interfaces;
using Application.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.Implementaions
{
    public class TelegramService : ITelegramService
    {
        #region Fields
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;
        #endregion

        #region Ctor
        public TelegramService(IConfiguration configuration,
            ILogger<TelegramService> logger)
        {
            _botClient = new TelegramBotClient(configuration["TelegramSettings:BotToken"]!);
            _logger = logger;
        }
        #endregion

        public async Task SendProductAlertAsync(SendProductAlertDto dto)
        {
            string message = BotMessages.TelegramAlert(dto.Title, dto.OldPrice, dto.NewPrice);
            var replyMarkup = BotMessages.GetProductReplyMarkup(dto.DigiKalaId);

            try
            {
                string fullPath = Path.Combine(dto.ImagePath ?? "", dto.ImageFileName ?? "");
                if (File.Exists(fullPath))
                {
                    using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    await _botClient.SendPhoto(
                        chatId: dto.ChatId,
                        photo: InputFile.FromStream(fileStream, dto.ImageFileName),
                        caption: message,
                        parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup
                    );
                    return;
                }
            }
            catch (Exception)
            {
            }

            await _botClient.SendMessage(dto.ChatId,
                text: message,
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup);
        }

        public async Task SendTextMessageAsync(SendTextMessageDto dto)
        {
            await _botClient.SendMessage(dto.ChatId, text: dto.Message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public async Task EditMessageReplyMarkupAsync(long chatId, int messageId, InlineKeyboardMarkup replyMarkup)
        {
            await _botClient.EditMessageReplyMarkup(chatId, messageId, replyMarkup);
        }

        public async Task EditMessageTextAsync(long chatId, int messageId, string newText)
        {
            await _botClient.EditMessageText(chatId, messageId, newText, parseMode: ParseMode.Html);
        }

        public async Task AnswerCallbackQueryAsync(string callbackQueryId, string text)
        {
            await _botClient.AnswerCallbackQuery(callbackQueryId, text);
        }

        public async Task SendTextMessageWithMarkupAsync(SendTextMessageWithMarkupDto dto)
        {
            await _botClient.SendMessage(
                chatId: dto.ChatId,
                text: dto.Text,
                parseMode: ParseMode.Html,
                replyMarkup: dto.ReplyMarkup
            );
        }

        public async Task DeleteMessageAsync(long chatId, int messageId)
        {
            try
            {
                await _botClient.DeleteMessage(chatId, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete message {MessageId} for chat {chatId}", messageId, chatId);
            }
        }
    }
}
