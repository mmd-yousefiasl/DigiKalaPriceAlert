using MediatR;

namespace Domain.Events.SocialMedia
{
    public record SocialMediaUpdatedEvent(
    int Id,
    string FileName,
    string Title,
    string Url,
    int Priority,
    bool IsPublished
    ) : INotification;
}
