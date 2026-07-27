using Domain.Models;

namespace Application.Dtos.SiteNotifications.AdminSide
{
    public class AdminSideFilterNotificationsDto : BasePaging<AdminSideNotificationDto>
    {
        public string? Search { get; set; }
    }
}
