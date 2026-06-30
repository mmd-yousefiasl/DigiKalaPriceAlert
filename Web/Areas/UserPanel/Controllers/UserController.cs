using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class UserController : BaseUserPanelController
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public UserController(IUserService userService)
        {
            _userService = userService;
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

        [HttpPost("/user/profile/edit"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ClientSideUpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _userService.ClientSideUpdateUserProfile(dto);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(dto);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction("AddNewProduct", "Product", new { area = "UserPanel" });
        }
    }
}