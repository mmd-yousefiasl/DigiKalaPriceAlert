using MediatR;

namespace Domain.Events.SocialMedia
{
    public record SocialMediaCreatedEvent(
    string FileName,
    string Title,
    string Url,
    int Priority,
    bool IsPublished
    ) : INotification;
}
