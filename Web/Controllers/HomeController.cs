using Application.Dtos.NewsLetter.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Xml.Linq;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        #region Fields
        private readonly INewsLetterService _newsLettersService;
        private readonly ISiteMapService _mapService;
        #endregion

        #region Ctor
        public HomeController(INewsLetterService newsLettersServices,
            ISiteMapService mapService)
        {
            _newsLettersService = newsLettersServices;
            _mapService = mapService;
        }
        #endregion

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/sitemap.xml")]
        public async Task<IActionResult> SiteMap()
        {
            var result = await _mapService.ClientSideGetSiteMap();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            var maps = result.Value; // IEnumerable<ClientSideSiteMapViewModel>

            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var xDoc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset",
                    maps.Select(m =>
                        new XElement(ns + "url",
                            new XElement(ns + "loc", m.Url),
                            new XElement(ns + "lastmod", m.LastUpdateDate.ToString("yyyy-MM-dd")),
                            new XElement(ns + "changefreq", m.ChangeFreq.GetDisplayName()),
                            new XElement(ns + "priority", m.Priority.ToString(CultureInfo.InvariantCulture))
                        )
                    )
                )
            );

            return Content(xDoc.ToString(), "application/xml");
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