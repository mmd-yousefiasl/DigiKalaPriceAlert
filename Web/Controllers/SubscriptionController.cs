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
        public IActionResult Purchase(int planId)
        {
            if (planId < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            return View();
        }

        [HttpGet("/subscription/pricing")]
        public IActionResult Pricing()
        {
            return View();
        }
    }
}
