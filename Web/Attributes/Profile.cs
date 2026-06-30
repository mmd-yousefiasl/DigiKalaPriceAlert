using Application.Services.Interfaces;
using Application.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Attributes
{
    public class Profile : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var _userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
            if (context.HttpContext.User?.Identity?.IsAuthenticated ?? false)
            {
                var userId = context.HttpContext.User.GetUserId();
                var membershipResult = await _userService.ClientSideIsUserProfileCompleted(userId);
                if (membershipResult.IsFailure)
                {
                    context.Result = new RedirectResult("/user/profile/edit");
                }
            }
            else
            {
                context.Result = new RedirectResult("/login");
            }
        }
    }
}
