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
        private readonly ITelegramUpdateQueue _telegramUpdateQueue;
        #endregion

        #region Ctor
        public TelegramController(ITelegramUpdateQueue telegramUpdateQueue)
        {
            _telegramUpdateQueue = telegramUpdateQueue;
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

            await _telegramUpdateQueue.EnqueueAsync(body);

            return Ok();
        }
    }
}