using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class SubscriptionController : BaseController
    {
        #region Fields
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor
        public SubscriptionController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        #endregion

        [Authorize]
        [HttpGet("/subscription/purchase/{planId}")]
        public async Task<IActionResult> Purchase(int planId)
        {
            if (planId < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(Pricing));
            }
            var result = await _orderService.ClientSideCreateNewPurchase(planId);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(Pricing));
            }

            return RedirectToAction(nameof(Invoice), new { id = result.Value });
        }

        [Authorize]
        [HttpGet("/subscription/get-invoice")]
        public async Task<IActionResult> GetInvoice()
        {
            var result = await _orderService.ClientSideGetInvoiceId();
            if (result.IsFailure)
            {
                return View();
            }

            return RedirectToAction(nameof(Invoice), new { id = result.Value });
        }

        [Authorize]
        [HttpGet("/subscription/invoice/{id}")]
        public async Task<IActionResult> Invoice(int id)
        {
            if (id < 1)
            {
                return RedirectToAction(nameof(GetInvoice));
            }
            var result = await _orderService.ClientSideGetInvoice(id);
            if (result.IsFailure)
            {
                return RedirectToAction(nameof(GetInvoice));
            }

            return View(result.Value);
        }

        [HttpGet("/subscription/pricing")]
        public IActionResult Pricing()
        {
            return View();
        }
    }
}
