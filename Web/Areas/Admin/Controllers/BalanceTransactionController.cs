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
        private readonly IBalanceTransactionService _transactionService;
        #endregion

        #region Ctor
        public BalanceTransactionController(IBalanceTransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        #endregion


        [Permission("BalanceTransactionsList")]
        [HttpGet("/admin/transactions-list")]
        public async Task<IActionResult> BalanceTransactionsList(AdminSideFilterBalanceTransactionsDto filter)
        {
            var result = await _transactionService.AdminSideFilterTransactions(filter);
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

            var result = await _transactionService.AdminSideCreateNewTransaction(model);
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
