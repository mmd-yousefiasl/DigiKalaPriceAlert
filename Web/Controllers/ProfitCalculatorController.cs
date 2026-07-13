using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class ProfitCalculatorController : BaseController
    {
        [HttpGet("/calculator")]
        public IActionResult Calculator()
        {
            ViewData["MetaDescription"] = "سود خالص فروش در دیجی‌کالا را با احتساب کمیسیون، مالیات بر ارزش افزوده، هزینه ارسال و مرجوعی به صورت آنلاین محاسبه کنید. ابزار رایگان دیجیتو.";
            return View();
        }
    }
}
