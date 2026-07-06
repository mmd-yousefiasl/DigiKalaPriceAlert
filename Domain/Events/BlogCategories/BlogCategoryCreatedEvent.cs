using MediatR;

namespace Domain.Events.BlogCategories
{
    public record BlogCategoryCreatedEvent(
    string Title,
    string Slug,
    int Priority,
    bool IsPublished
    ) : INotification;
}
