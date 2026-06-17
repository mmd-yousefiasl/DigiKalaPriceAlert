using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Components
{
    public class ClientSideLatestBalanceTransactionsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IBalanceTransactionService _balanceTransactionService;
        #endregion

        #region Ctor
        public ClientSideLatestBalanceTransactionsViewComponent(IBalanceTransactionService balanceTransactionService)
        {
            _balanceTransactionService = balanceTransactionService;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _balanceTransactionService.ClientSideFilterTransactions(new ClientSideFilterBalanceTransactionsDto()
            {
                TakeEntity = 50
            });
            if (result.IsFailure)
                return View("ClientSideLatestBalanceTransactions", new ClientSideFilterBalanceTransactionsDto());

            return View("ClientSideLatestBalanceTransactions", result.Value);
        }
    }
}
