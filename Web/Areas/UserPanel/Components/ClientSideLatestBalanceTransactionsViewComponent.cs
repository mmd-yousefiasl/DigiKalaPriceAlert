using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Components
{
    public class ClientSideLatestBalanceTransactionsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public ClientSideLatestBalanceTransactionsViewComponent(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _userService.ClientSideFilterTransactions(new ClientSideFilterBalanceTransactionsDto()
            {
                TakeEntity = 15
            });
            if (result.IsFailure)
                return View("ClientSideLatestBalanceTransactions", new ClientSideFilterBalanceTransactionsDto());

            return View("ClientSideLatestBalanceTransactions", result.Value);
        }
    }
}
