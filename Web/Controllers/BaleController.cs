using Application.Dtos.Bale;
using Application.Services.Interfaces;
using Domain.Statics;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaleController : BaseController
    {
        private readonly IBaleUpdateQueue _queue;

        public BaleController(IBaleUpdateQueue queue)
        {
            _queue = queue;
        }

        [HttpPost("webhook/{secret}")]
        public async Task<IActionResult> ReceiveWebhook(string secret, [FromBody] BaleUpdateDto update, CancellationToken ct)
        {
            if (!SecureEquals(secret, SiteTools.BaleSecretToken))
                return Unauthorized();

            await _queue.EnqueueAsync(update, ct);
            return Ok();
        }

        private static bool SecureEquals(string? a, string? b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);

            return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
        }
    }
}
