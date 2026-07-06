using MediatR;

namespace Domain.Events.Blogs
{
    public record BlogCreatedEvent(
    string FileName,
    string Title,
    string Slug,
    string Description,
    string Writer,
    string StudyTime,
    int Priority,
    bool IsPublished,
    int CategoryId
    ) : INotification;
}
