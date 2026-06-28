using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageBalanceTransaction")]
    public class BalanceTransactionController : BaseAdminController
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public BalanceTransactionController(IUserService userService)
        {
            _userService = userService;
        }
        #endregion


        [Permission("BalanceTransactionsList")]
        [HttpGet("/admin/transactions-list")]
        public async Task<IActionResult> BalanceTransactionsList(AdminSideFilterBalanceTransactionsDto filter)
        {
            var result = await _userService.AdminSideFilterTransactions(filter);
            if (result.IsFailure)
                return View(new AdminSideFilterBalanceTransactionsDto() { UserId = filter.UserId });

            return View(result.Value);
        }

        [Permission("CreateNewBalanceTransaction")]
        [HttpGet("/admin/add-new-transaction")]
        public IActionResult CreateNewBalanceTransaction(int userId)
        {
            return View(new AdminSideCreateNewBalanceTransactionDto() { UserId = userId });
        }

        [Permission("CreateNewBalanceTransaction")]
        [HttpPost("/admin/add-new-transaction"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewBalanceTransaction(AdminSideCreateNewBalanceTransactionDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _userService.AdminSideCreateNewTransaction(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction("UserDetails", "User", new { id = model.UserId });
        }
    }
}
