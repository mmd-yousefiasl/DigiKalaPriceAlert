using Application.Dtos.BalanceTransaction.AdminSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Components
{
    public class AdminSideLatestBalanceTransactionsViewComponent : ViewComponent
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public AdminSideLatestBalanceTransactionsViewComponent(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync(int userId)
        {
            var result = await _userService.AdminSideFilterTransactions(new AdminSideFilterBalanceTransactionsDto()
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
