using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Components
{
    public class ClientSideUserBalanceViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        public ClientSideUserBalanceViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool showLink)
        {
            var balance = await _userService.ClientSideGetCurrentUserBalance();
            return View("ClientSideUserBalance", new ClientSideUserBalanceDto()
            {
                Balance = balance,
                ShowLink = showLink
            });
        }
    }
}
