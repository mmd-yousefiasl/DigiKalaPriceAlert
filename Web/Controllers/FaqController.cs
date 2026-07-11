using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class FaqController : BaseController
    {
        #region Fields
        private readonly IFaqService _faqService;
        #endregion

        #region Ctor
        public FaqController(IFaqService faqService)
        {
            _faqService = faqService;
        }
        #endregion

        [HttpGet("/faq")]
        public async Task<IActionResult> Faq()
        {
            var result = await _faqService.GetFaqs();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return Redirect("/");
            }

            ViewData["MetaAuthor"] = "تیم استراتژی و فنی دیجی‌پرایس‌الرت";
            ViewData["MetaDescription"] = "پاسخ به سوالات متداول فروشندگان؛ هر آنچه درباره نحوه پایش اتوماتیک قیمت‌ها، امنیت پنل و چگونگی تصاحب بای‌باکس با دیجی‌پرایس‌الرت باید بدانید.";

            return View(result.Value);
        }
    }
}
