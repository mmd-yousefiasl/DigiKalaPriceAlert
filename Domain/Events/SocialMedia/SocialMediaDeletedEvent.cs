using MediatR;

namespace Domain.Events.SocialMedia
{
    public record SocialMediaDeletedEvent(
        int Id
        ) : INotification;
}
