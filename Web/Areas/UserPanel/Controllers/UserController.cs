using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class UserController : BaseUserPanelController
    {
        #region Fields
        private readonly IBalanceTransactionService _balanceTransactionService;
        #endregion

        #region Ctor
        public UserController(IBalanceTransactionService balanceTransactionService)
        {
            _balanceTransactionService = balanceTransactionService;
        }
        #endregion

        [HttpGet("/user/transactions")]
        public async Task<IActionResult> MyTransactions(ClientSideFilterBalanceTransactionsDto filter)
        {
            var result = await _balanceTransactionService.ClientSideFilterTransactions(filter);
            if (result.IsFailure)
                return View(new ClientSideFilterBalanceTransactionsDto());

            return View(result.Value);
        }

        [HttpGet("/user/deposit")]
        public IActionResult Deposit()
        {
            return View();
        }
    }
}
