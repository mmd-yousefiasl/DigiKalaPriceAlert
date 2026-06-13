using Application.Dtos.ContactUs.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ContactUsController : BaseController
    {
        #region Fields
        private readonly IContactUsFormService _contactUsFormService;
        #endregion

        #region Ctor
        public ContactUsController(IContactUsFormService contactUsFormService)
        {
            _contactUsFormService = contactUsFormService;
        }
        #endregion

        [HttpGet("/contact-us")]
        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost("/contact-us"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(ClientSideSendNewContactUsFormDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _contactUsFormService.ClientSideSendNewContactUsForm(dto);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(dto);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(ContactUs));
        }
    }
}
