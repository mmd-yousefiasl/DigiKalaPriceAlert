using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
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

        [HttpGet("/pricing")]
        public IActionResult Pricing()
        {
            return View();
        }

        [HttpGet("/about-us")]
        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet("/contact-us")]
        public IActionResult ContactUs()
        {
            return View();
        }
    }
}