using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Admin.Components
{
    public class AdminNotificationsCountVerticalMenuViewComponent : ViewComponent
    {
        private readonly ISiteNotificationService _notificationService;
        public AdminNotificationsCountVerticalMenuViewComponent(ISiteNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _notificationService.AdminSideGetUnreadNotificationsCount();

            return View("AdminNotificationsCountVerticalMenu", result);
        }
    }
}
