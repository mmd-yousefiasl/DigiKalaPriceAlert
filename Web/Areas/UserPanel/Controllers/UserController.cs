using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class UserController : BaseUserPanelController
    {
        #region Fields
        private readonly IUserService _userService;
        private readonly ITelegramService _telegramService;
        private readonly IBaleService _baleService;
        #endregion

        #region Ctor
        public UserController(IUserService userService,
            ITelegramService telegramService,
            IBaleService baleService)
        {
            _userService = userService;
            _telegramService = telegramService;
            _baleService = baleService;
        }
        #endregion

        [HttpGet("/user/transactions")]
        public async Task<IActionResult> MyTransactions(ClientSideFilterBalanceTransactionsDto filter)
        {
            var result = await _userService.ClientSideFilterTransactions(filter);
            if (result.IsFailure)
                return View(new ClientSideFilterBalanceTransactionsDto());

            return View(result.Value);
        }

        [HttpGet("/user/profile/edit")]
        public IActionResult UpdateProfile()
        {
            return View();
        }

        [HttpPost("/user/profile/check"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckProfile()
        {
            var result = await _userService.ClientSideIsUserProfileCompleted();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(UpdateProfile));
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction("AddNewProduct", "Product", new { area = "UserPanel" });
        }

        [HttpPost("/user/profile/connect-bale"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectBale()
        {
            var result = await _userService.ClientSideConnectUserToBale();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(UpdateProfile));
            }

            return Redirect(result.Value);
        }

        [HttpPost("/user/profile/send-bale-test-message"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBaleTestMessage()
        {
            var result = await _baleService.SendBaleTestMessage();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = Messages.TestMessageSentSuccessfully;
            }

            return RedirectToAction(nameof(UpdateProfile));
        }

        [HttpGet("/user/my-info")]
        public async Task<IActionResult> MyInfo()
        {
            var result = await _userService.ClientSideGetUserProfileForUpdate();
            if (result.IsFailure)
                return View(new ClientSideUpdateProfileDto());

            return View(result.Value);
        }

        [HttpPost("/user/my-info"), ValidateAntiForgeryToken]
        public async Task<IActionResult> MyInfo(ClientSideUpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.ClientSideUpdateUserProfile(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(MyInfo));
        }

        [HttpPost("/user/my-info/send-bale-test-message"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SendBaleTestMessageFromMyInfo()
        {
            var result = await _baleService.SendBaleTestMessage();

            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = Messages.TestMessageSentSuccessfully;
            }

            return RedirectToAction(nameof(MyInfo));
        }
    }
}