using MediatR;

namespace Domain.Events.Users
{
    public record UserUpdatedEvent(
    int Id
    ) : INotification;
}
