using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Components
{
    public class ClientSIdeUserSubscriptionsViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        public ClientSIdeUserSubscriptionsViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _userService.ClientSideCurrentUserSubscriptions();
            if (result.IsFailure)
                return View("ClientSIdeUserSubscriptions", new ClientSideUserSubscriptionsDto());

            return View("ClientSIdeUserSubscriptions", result.Value);
        }
    }
}
