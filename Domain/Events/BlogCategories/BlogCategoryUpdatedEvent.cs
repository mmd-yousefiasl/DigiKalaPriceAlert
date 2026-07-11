using MediatR;

namespace Domain.Events.BlogCategories
{
    public record BlogCategoryUpdatedEvent(
    int Id,
    string Title,
    string Slug,
    string MetaDescription,
    int Priority,
    bool IsPublished
    ) : INotification;
}
