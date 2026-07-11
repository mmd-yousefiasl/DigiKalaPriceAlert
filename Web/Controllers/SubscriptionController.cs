using Application.Dtos.Payment;
using Application.Services.Interfaces;
using Domain.Enums.Payment;
using Domain.Shared;
using Domain.Statics;
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
            ViewData["NoIndex"] = true;
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
            ViewData["NoIndex"] = true;
            var result = await _orderService.ClientSideGetInvoiceId();
            if (result.IsFailure)
            {
                return View();
            }

            return RedirectToAction(nameof(Invoice), new { id = result.Value });
        }

        [Authorize]
        [HttpGet("/subscription/invoice/{id}")]
        public async Task<IActionResult> Invoice(string id)
        {
            ViewData["NoIndex"] = true;
            if (string.IsNullOrWhiteSpace(id))
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

        [Authorize]
        [HttpPost("/subscription/checkout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(bool useBalance)
        {
            ViewData["NoIndex"] = true;
            var result = await _orderService.ClientSideProceedToCheckout(useBalance);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(GetInvoice));
            }
            if (result.Value.IsDone)
                return RedirectToAction(nameof(VerifyOrder));

            return Redirect(SiteTools.StartPaymentURL + result.Value.TransId);
        }

        [AllowAnonymous]
        [HttpGet("/subscription/payment-result")]
        public async Task<IActionResult> PaymentResult(PaymentResultDto model)
        {
            ViewData["NoIndex"] = true;
            if (!ModelState.IsValid || model.status != PaymentResultStatusEnum.PaidButNotVerified)
            {
                TempData[TemporaryErrorMessage] = Messages.PaymentFailedError;
                return RedirectToAction(nameof(PaymentFailed));
            }
            var result = await _orderService.ClientSideCheckout(model);
            if (result.IsFailure)
            {
                return RedirectToAction(nameof(PaymentFailed));
            }

            return RedirectToAction(nameof(VerifyOrder));
        }

        [HttpGet("/subscription/verify-order")]
        public async Task<IActionResult> VerifyOrder()
        {
            ViewData["NoIndex"] = true;
            var result = await _orderService.ClientSideVerifyOrder();
            if (result.IsFailure) return RedirectToAction(nameof(PaymentFailed));

            return RedirectToAction(nameof(PaymentSuccess), new { id = result.Value });
        }

        [HttpGet("/subscription/pricing")]
        public IActionResult Pricing()
        {
            ViewData["MetaAuthor"] = "تیم استراتژی و فنی دیجی‌پرایس‌الرت";
            ViewData["MetaDescription"] = "تعرفه اشتراک و قیمت پکیج‌های پایش قیمت. با انتخاب پلن هوشمند، از رقابت فرسایشی جلوگیری کرده و حاشیه سود خالص خود را در دیجی‌کالا تضمین کنید. ";
            return View();
        }

        [Authorize]
        [HttpGet("/subscription/payment/success/{id}")]
        public IActionResult PaymentSuccess(string id)
        {
            ViewData["NoIndex"] = true;
            return View(nameof(PaymentSuccess), id);
        }

        [Authorize]
        [HttpGet("/subscription/payment/failed")]
        public IActionResult PaymentFailed()
        {
            ViewData["NoIndex"] = true;
            return View();
        }
    }
}
