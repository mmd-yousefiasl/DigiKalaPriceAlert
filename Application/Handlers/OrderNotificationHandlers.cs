using Application.Dtos.SiteNotifications.AdminSide;
using Application.Services.Interfaces;
using Application.Tools;
using Domain.Enums.SiteNotifications;
using Domain.Events.Orders;
using MediatR;

namespace Application.Handlers
{
    public class OrderNotificationHandlers : INotificationHandler<OrderUpdatedEvent>
    {
        #region Fields
        private readonly ISiteNotificationService _notificationService;
        #endregion

        #region Ctor
        public OrderNotificationHandlers(ISiteNotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        #endregion

        public async Task Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var orderNotif = new AdminSideCreateNewNotificationDto()
            {
                Type = SiteNotificationTypeEnum.NewOrder,
                Content = $"سفارش #{notification.Id} توسط کاربر #{notification.UserId} " +
                          $"برای پلن «{notification.PlanTitle ?? "نامشخص"}» " +
                          $"به مبلغ {notification.PlanPrice?.ToToman() ?? "نامشخص"} " +
                          $"با وضعیت «{notification.Status.GetDisplayName()}» " +
                          $"در تاریخ {notification.CreatedDate.GetDateTime()} ثبت شد. " +
                          $"شناسه فاکتور: {notification.InvoiceId}"
            };

            await _notificationService.AdminSideCreateNewNotification(orderNotif);
        }
    }
}
