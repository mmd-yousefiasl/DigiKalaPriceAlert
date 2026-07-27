using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Components
{
    public class AdminUnreadNotificationsCountViewComponent : ViewComponent
    {
        private readonly ISiteNotificationService _notificationService;
        public AdminUnreadNotificationsCountViewComponent(ISiteNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _notificationService.AdminSideGetUnreadNotificationsCount();

            return View("AdminUnreadNotificationsCount", result);
        }
    }
}
