using Application.Dtos.Products.BotSide;
using Application.Dtos.TelegramDto;
using Application.Dtos.Users.BotSide;
using Application.Services.Interfaces;
using Application.Tools;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Application.Services.Implementaions
{
    public class TelegramUpdateHandler : ITelegramUpdateHandler
    {
        #region Fields
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly ITelegramService _telegramService;
        private readonly IDigiKalaService _digiKalaService;
        #endregion

        #region Ctor
        public TelegramUpdateHandler(
            IProductService productService,
            IUserService userService,
            ITelegramService telegramService,
            IDigiKalaService digiKalaService)
        {
            _productService = productService;
            _userService = userService;
            _telegramService = telegramService;
            _digiKalaService = digiKalaService;
        }
        #endregion

        public async Task HandleUpdateAsync(Update update)
        {
            // جلوگیری از پاسخ‌دهی به بات‌های دیگر
            if (update.Message?.From?.IsBot ?? false) return;

            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(update.CallbackQuery);
                return;
            }

            if (update.Type == UpdateType.Message && update.Message != null)
            {
                if (update.Message.Text != null || update.Message.Contact != null)
                {
                    await HandleMessageAsync(update.Message);
                }
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim();
            var user = message.From;

            #region Check User Account
            var userExist = await _userService.BotSideDoesUserExist(chatId);
            if (userExist.IsFailure && message.Contact == null)
            {
                await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                {
                    ChatId = chatId,
                    Text = BotMessages.ShareYourPhoneNumberToCreateAccountAndConnectToWebsite,
                    ReplyMarkup = BotMessages.GetUserContactMarkup(),
                });

                return;
            }
            else if (userExist.IsFailure && message.Contact != null)
            {
                if (message.Contact.UserId != chatId)
                {
                    await _telegramService.SendTextMessageAsync(new SendTextMessageDto
                    {
                        ChatId = chatId,
                        Message = BotMessages.InvalidContactError
                    });
                    return;
                }

                var registerUserResult = await _userService.BotSideCreateNewUser(new BotSideCreateNewUserDto
                {
                    Mobile = message.Contact.PhoneNumber,
                    ChatId = chatId,
                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    UserName = user?.Username
                });
                if (registerUserResult.IsFailure)
                {
                    await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                    {
                        ChatId = chatId,
                        Message = registerUserResult.Message!
                    });
                    return;
                }
                else
                {
                    await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                    {
                        ChatId = chatId,
                        Text = BotMessages.WelcomeMessage(user?.FirstName),
                        ReplyMarkup = BotMessages.GetMainMenuMarkup()
                    });
                    return;
                }
            }
            #endregion

            if (string.IsNullOrEmpty(text)) return;

            #region Start
            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase) || text == BotMessages.MenuBack)
            {
                await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                {
                    ChatId = chatId,
                    Text = BotMessages.WelcomeMessage(user?.FirstName),
                    ReplyMarkup = BotMessages.GetMainMenuMarkup()
                });
                return;
            }
            #endregion

            #region AboutUs
            if (text == BotMessages.MenuAboutUs)
            {
                await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                {
                    ChatId = chatId,
                    Message = BotMessages.AboutUsMessage
                });
                return;
            }
            #endregion

            #region MyProducts
            if (text == BotMessages.MenuMyProducts)
            {
                var userProductsResult = await _productService.GetUserProducts(chatId);

                if (userProductsResult.IsFailure)
                {
                    await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                    {
                        ChatId = chatId,
                        Message = userProductsResult.Message!
                    });
                    return;
                }

                var userProductsListMarkup = BotMessages.GetUserProductsListMarkup(userProductsResult.Value);

                await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                {
                    ChatId = chatId,
                    Text = BotMessages.UserProductsWatchListTitle(userProductsResult.Value.Count),
                    ReplyMarkup = userProductsListMarkup
                });

                return;
            }
            #endregion

            #region MyAccount
            if (text == BotMessages.MenuMyAccount)
            {
                var subscriptionDetailsResult = await _userService.GetCurrenUserSubscriptionPlanByChatId(chatId);
                if (subscriptionDetailsResult.IsFailure)
                {
                    await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                    {
                        ChatId = chatId,
                        Message = subscriptionDetailsResult.Message!
                    });
                    return;
                }
                else
                {
                    await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                    {
                        ChatId = chatId,
                        Text = BotMessages.SubscriptionStatusMessage(subscriptionDetailsResult.Value),
                        ReplyMarkup = BotMessages.GetSubscriptionUpgradeReplyMarkup("https://google.com")
                    });
                }

                return;
            }
            #endregion

            #region Add Product To Watch List
            // بررسی می‌کنیم که آیا متن شامل یک یا چند ورودی معتبر هست یا خیر
            var validInputs = text.Split(new[] { ' ', '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Where(t => t.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                                              t.StartsWith("dkp", StringComparison.OrdinalIgnoreCase) ||
                                              int.TryParse(t, out _))
                                  .ToList();

            if (validInputs.Any())
            {
                string searchMsg = validInputs.Count > 1
                    ? BotMessages.SearchingMultipleDigiKalaMessage(validInputs.Count)
                    : BotMessages.SearchingDigiKalaMessage;

                await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                {
                    ChatId = chatId,
                    Message = searchMsg
                });

                foreach (var input in validInputs)
                {
                    var canUserAddProduct = await _userService.BotSideCanUserAddProduct(chatId);
                    if (canUserAddProduct.IsFailure)
                    {
                        await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                        {
                            ChatId = chatId,
                            Text = canUserAddProduct.Message!,
                            ReplyMarkup = BotMessages.GetSubscriptionLimitReplyMarkup()
                        });
                        break;
                    }

                    var digiKalaIdResult = _digiKalaService.ExtractProductIdFromUrl(input);
                    if (digiKalaIdResult.IsFailure)
                    {
                        await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                        {
                            ChatId = chatId,
                            Message = digiKalaIdResult.Message!
                        });
                        continue;
                    }

                    BotSideProductDto watchListProduct;
                    var getProdcutResult = await _productService.GetProductByDigiKalaId(digiKalaIdResult.Value);

                    if (getProdcutResult.IsFailure)
                    {
                        var createdProductResult = await _productService.CreateProductForUser(new BotSideCreateProductForUserDto()
                        {
                            ChatId = chatId,
                            DigiKalaId = digiKalaIdResult.Value
                        });

                        if (createdProductResult.IsFailure)
                        {
                            await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                            {
                                ChatId = chatId,
                                Message = createdProductResult.Message!
                            });
                            continue;
                        }
                        watchListProduct = createdProductResult.Value;
                    }
                    else
                    {
                        await _userService.BotSideCreateNewUserProduct(new BotSideCreateNewUserProductDto()
                        {
                            ChatId = chatId,
                            ProductId = getProdcutResult.Value.Id
                        });
                        watchListProduct = getProdcutResult.Value;
                    }

                    await _telegramService.SendProductAlertAsync(new SendProductAlertDto()
                    {
                        ChatId = chatId,
                        DigiKalaId = digiKalaIdResult.Value,
                        ImageFileName = watchListProduct.ImageFileName,
                        ImagePath = AppInfo.ImageCacheFolder,
                        NewPrice = watchListProduct.NewPrice,
                        OldPrice = watchListProduct.OldPrice,
                        Title = watchListProduct.Title
                    });

                    if (validInputs.Count > 1) await Task.Delay(50);
                }

                return;
            }
            #endregion

            await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
            {
                ChatId = chatId,
                Message = BotMessages.InvalidOrderError
            });
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data)) return;

            var chatId = callbackQuery.Message!.Chat.Id;
            var messageId = callbackQuery.Message.MessageId;

            var parts = data.Split(':');
            var action = parts[0];
            var digiKalaId = parts.Length > 1 ? parts[1] : string.Empty;

            switch (action)
            {
                case "confirm_delete":
                    await _telegramService.EditMessageReplyMarkupAsync(chatId, messageId,
                        BotMessages.GetDeleteConfirmationMarkup(digiKalaId));
                    await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, BotMessages.ConfirmProductDeleteMessage);
                    break;

                case "view_product":
                    var productResult = await _productService.GetProductForUserByDigiKalaId(new BotSideGetProductForUserDto()
                    {
                        ChatId = chatId,
                        DigiKalaId = digiKalaId
                    });

                    if (productResult.IsFailure)
                    {
                        await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, productResult.Message!);
                        return;
                    }

                    var product = productResult.Value;
                    await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, $"📦 نمایش: {product.Title}");

                    await _telegramService.SendProductAlertAsync(new SendProductAlertDto()
                    {
                        Title = product.Title,
                        DigiKalaId = product.DigiKalaId,
                        ImageFileName = product.ImageFileName,
                        ImagePath = AppInfo.ImageCacheFolder,
                        NewPrice = product.NewPrice,
                        OldPrice = product.OldPrice,
                        ChatId = chatId
                    });
                    break;

                case "delete_yes":
                    var result = await _productService.RemoveProductFromUserAsync(new BotSideProductUserInfoDto()
                    {
                        ChatId = chatId,
                        DigiKalaId = digiKalaId
                    });

                    if (result.IsFailure)
                    {
                        await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, result.Message!);
                    }
                    else
                    {
                        await _telegramService.DeleteMessageAsync(chatId, messageId);
                        await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, result.Message!);
                    }
                    break;

                case "delete_no":
                    await _telegramService.EditMessageReplyMarkupAsync(chatId, messageId,
                        BotMessages.GetProductReplyMarkup(digiKalaId));
                    await _telegramService.AnswerCallbackQueryAsync(callbackQuery.Id, BotMessages.OperationCancelledMessage);
                    break;

                case "view_subscription_status":
                    var subscriptionDetailsResult = await _userService.GetCurrenUserSubscriptionPlanByChatId(chatId);
                    if (subscriptionDetailsResult.IsFailure)
                    {
                        await _telegramService.SendTextMessageAsync(new SendTextMessageDto()
                        {
                            ChatId = chatId,
                            Message = subscriptionDetailsResult.Message!
                        });
                        return;
                    }
                    else
                    {
                        await _telegramService.DeleteMessageAsync(chatId, messageId);
                        await _telegramService.SendTextMessageWithMarkupAsync(new SendTextMessageWithMarkupDto()
                        {
                            ChatId = chatId,
                            Text = BotMessages.SubscriptionStatusMessage(subscriptionDetailsResult.Value),
                            ReplyMarkup = BotMessages.GetSubscriptionUpgradeReplyMarkup("https://google.com")
                        });
                    }
                    break;
            }
        }
    }
}