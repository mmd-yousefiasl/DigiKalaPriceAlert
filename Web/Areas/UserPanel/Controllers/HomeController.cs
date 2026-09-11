using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class HomeController : BaseUserPanelController
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public HomeController(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        [HttpGet("/user")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpGet("/user/subscription-expired")]
        public IActionResult SubscriptionExpired()
        {
            return View();
        }
    }
}
