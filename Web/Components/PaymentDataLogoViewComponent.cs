using Application.Dtos.PaymentData;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Components
{
    public class PaymentDataLogoViewComponent : ViewComponent
    {
        private readonly IPaymentDataService _dataService;
        public PaymentDataLogoViewComponent(IPaymentDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _dataService.ClientSideGetPaymentDataLogo();
            if (result.IsFailure) return View("PaymentDataLogo", new ClientSidePaymentDataLogoDto());

            return View("PaymentDataLogo", result.Value);
        }
    }
}
