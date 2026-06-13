using Application.Dtos.ContactUs.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageContactUsForms")]
    public class ContactUsController : BaseAdminController
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

        [Permission("ContactUsFormsList")]
        [HttpGet("/admin/contact-us-forms")]
        public async Task<IActionResult> ContactUsFormsList(AdminSideFilterContactUsFormsDto filter)
        {
            var result = await _contactUsFormService.AdminSideFilterContactUsFomrs(filter);
            if (result.IsFailure) return View(new AdminSideFilterContactUsFormsDto());

            return View(result.Value);
        }

        [Permission("DeleteContactUsForm")]
        [HttpPost("/admin/remove-contact-us-form"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContactUsForm(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(ContactUsFormsList));
            }
            var result = await _contactUsFormService.AdminSideDeleteContactUsForm(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(ContactUsFormsList));
        }

        [Permission("ReadContactUsForm")]
        [HttpPost("/admin/read-contact-us-form"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadContactUsForm(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(ContactUsFormsList));
            }
            var result = await _contactUsFormService.AdminSideCheckContactUsFormAsRead(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(ContactUsFormsList));
        }
    }
}