using MediatR;

namespace Domain.Events.Users
{
    public record UserDeletedEvent(
    int Id
    ) : INotification;
}
