using Application.Dtos.SentLetter.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageSentLetters")]
    public class SentLetterController : BaseAdminController
    {
        #region Fields
        private readonly ISentLetterService _sentLetterService;
        private readonly INewsLetterService _newsLettersService;
        #endregion

        #region Ctor
        public SentLetterController(ISentLetterService sentLetterService,
            INewsLetterService newsLettersService)
        {
            _sentLetterService = sentLetterService;
            _newsLettersService = newsLettersService;
        }
        #endregion

        [Permission("SentLettersList")]
        [HttpGet("/admin/sent-letters-list")]
        public async Task<IActionResult> SentLettersList(AdminSideFilterSentLettersDto filter)
        {
            var result = await _sentLetterService.AdminSideFilterSentLetters(filter);
            if (result.IsFailure) return View(new AdminSideFilterSentLettersDto());

            return View(result.Value);
        }

        [Permission("DeleteSentLetter")]
        [HttpPost("/admin/remove-sent-letter"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSentLetter(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(SentLettersList));
            }

            var result = await _sentLetterService.AdminSideDeleteSentLetter(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SentLettersList));
        }

        [Permission("SendLetter")]
        [HttpGet("/admin/send-letter")]
        public IActionResult SendLetter()
        {
            return View();
        }

        [Permission("SendLetter")]
        [HttpPost("/admin/send-letter"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SendLetter(AdminSideSendLetterDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _newsLettersService.AdminSideSendNews(dto);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(dto);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(SentLettersList));
        }
    }
}
