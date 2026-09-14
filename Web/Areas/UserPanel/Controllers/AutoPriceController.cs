using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class AutoPriceController : BaseUserPanelController
    {
        [HttpGet("/user/auto-price/products")]
        public IActionResult PanelProductsList()
        {
            return View();
        }
    }
}
