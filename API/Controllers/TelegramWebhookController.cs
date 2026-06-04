using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace API.Controllers
{
    [ApiController]
    [Route("api/webhook/telegram")]
    public class TelegramWebhookController : ControllerBase
    {
        #region Fields
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TelegramWebhookController> _logger;
        #endregion

        #region Ctor
        public TelegramWebhookController(
            IServiceScopeFactory scopeFactory,
            ILogger<TelegramWebhookController> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        #endregion

        [HttpPost]
        public IActionResult ReceiveUpdate([FromBody] Update update)
        {
            _ = Task.Run(async () =>
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        var updateHandler = scope.ServiceProvider.GetRequiredService<ITelegramUpdateHandler>();

                        await updateHandler.HandleUpdateAsync(update);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred during processing Telegram Update ID: {UpdateId}", update.Id);
                    }
                }
            });

            return Ok();
        }
    }
}
