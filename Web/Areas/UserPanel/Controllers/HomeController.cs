using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class HomeController : BaseUserPanelController
    {
        [HttpGet("/user")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
