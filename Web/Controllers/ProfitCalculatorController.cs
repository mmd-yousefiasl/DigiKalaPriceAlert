using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ProfitCalculatorController : BaseController
    {
        [HttpGet("/calculator")]
        public IActionResult Calculator()
        {
            return View();
        }
    }
}
