using MediatR;

namespace Domain.Events.BlogCategories
{
    public record BlogCategoryDeletedEvent(
    int Id
    ) : INotification;
}
