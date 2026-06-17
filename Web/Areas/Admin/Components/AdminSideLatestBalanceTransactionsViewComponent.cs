using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Components
{
    public class AdminSideLatestBalanceTransactionsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IBalanceTransactionService _balanceTransactionService;
        #endregion

        #region Ctor
        public AdminSideLatestBalanceTransactionsViewComponent(IBalanceTransactionService balanceTransactionService)
        {
            _balanceTransactionService = balanceTransactionService;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync(int userId)
        {
            var result = await _balanceTransactionService.AdminSideFilterTransactions(new AdminSideFilterBalanceTransactionsDto()
            {
                UserId = userId,
                TakeEntity = 50
            });
            if (result.IsFailure)
                return View("AdminSideLatestBalanceTransactions", new AdminSideFilterBalanceTransactionsDto());

            return View("AdminSideLatestBalanceTransactions", result.Value);
        }
    }
}
