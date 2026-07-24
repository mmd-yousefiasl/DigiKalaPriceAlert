using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.SiteNotifications.AdminSide
{
    public class AdminSideCreateNewNotificationDto
    {
        [MaxLength(2500)]
        public string Content { get; set; }
    }
}
