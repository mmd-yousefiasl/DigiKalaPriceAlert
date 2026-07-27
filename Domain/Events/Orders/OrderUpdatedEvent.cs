using Domain.Enums.Orders;
using MediatR;

namespace Domain.Events.Orders
{
    public record OrderUpdatedEvent(
    int Id,
    DateTime CreatedDate,
    DateTime LastUpdatedDate,
    OrderStatusEnum Status,
    decimal? PlanPrice,
    string? PlanTitle,
    DateTime? PaidDate,
    string InvoiceId,
    int SubscriptionPlanId,
    int UserId
    ) : INotification;
}