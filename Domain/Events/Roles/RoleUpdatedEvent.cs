using MediatR;

namespace Domain.Events.Roles
{
    public record RoleUpdatedEvent(
    int Id,
    string Title,
    string UniqueName
    ) : INotification;
}
