using Application.Dtos.NewsLetter.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        #region Fields
        private readonly INewsLetterService _newsLettersService;
        #endregion

        #region Ctor
        public HomeController(INewsLetterService newsLettersServices)
        {
            _newsLettersService = newsLettersServices;
        }
        #endregion

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/features")]
        public IActionResult Features()
        {
            return View();
        }

        [HttpGet("/about-us")]
        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet("/404")]
        public IActionResult PageNotFound()
        {
            ViewData["NoIndex"] = true;
            return View();
        }

        [HttpGet("/403")]
        public IActionResult AccessDenied()
        {
            ViewData["NoIndex"] = true;
            return View();
        }

        [HttpGet("/500")]
        public IActionResult InternalServerError()
        {
            ViewData["NoIndex"] = true;
            return View();
        }

        [HttpGet("/privacy-policy")]
        public IActionResult PrivacyPolicy()
        {
            ViewData["NoIndex"] = true;

            return View();
        }

        [HttpGet("/terms-and-conditions")]
        public IActionResult TermsAndConditions()
        {
            ViewData["NoIndex"] = true;

            return View();
        }

        [HttpPost("/sign-in-news-letters"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInNewsLetters(ClientSideSignInNewsLettersDto model)
        {
            if (!ModelState.IsValid) return View(nameof(Index), model);

            var result = await _newsLettersService.ClientSideSignInNewsLetters(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}