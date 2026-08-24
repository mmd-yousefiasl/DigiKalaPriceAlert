using Application.Dtos.BalanceTransaction.ClientSide;
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
        #endregion

        #region Ctor
        public UserController(IUserService userService,
            ITelegramService telegramService)
        {
            _userService = userService;
            _telegramService = telegramService;
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

        [HttpPost("/user/profile/connect-telegram"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectTelegram()
        {
            var result = await _userService.ClientSideConnectUserToTelegram();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(UpdateProfile));
            }

            return Redirect(result.Value);
        }

        [HttpPost("/user/profile/send-telegram-test-message"), ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTelegramTestMessage()
        {
            var result = await _telegramService.SendTelegramTestMessage();
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
    }
}