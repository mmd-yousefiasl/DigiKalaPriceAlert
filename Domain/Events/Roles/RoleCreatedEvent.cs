using MediatR;

namespace Domain.Events.Roles
{
    public record RoleCreatedEvent(
    string Title,
    string UniqueName
    ) : INotification;
}
