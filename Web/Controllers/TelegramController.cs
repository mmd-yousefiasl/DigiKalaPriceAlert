using Application.Services.Interfaces;
using Domain.Statics;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramController : BaseController
    {
        #region Fields
        private readonly ITelegramService _telegramService;
        #endregion

        #region Ctor
        public TelegramController(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }
        #endregion

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveWebhook()
        {
            if (!Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var headerValue) ||
                headerValue != SiteTools.TelegramWorkerKey)
            {
                return Unauthorized();
            }

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Return immediately
            _ = Task.Run(() => _telegramService.ProcessTelegramUpdateAsync(body));

            return Ok();
        }
    }
}