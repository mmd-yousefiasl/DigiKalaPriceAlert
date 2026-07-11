using MediatR;

namespace Domain.Events.BlogCategories
{
    public record BlogCategoryCreatedEvent(
    string Title,
    string Slug,
    string MetaDescription,
    int Priority,
    bool IsPublished
    ) : INotification;
}
