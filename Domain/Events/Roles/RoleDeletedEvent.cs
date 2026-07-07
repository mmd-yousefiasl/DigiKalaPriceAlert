using MediatR;

namespace Domain.Events.Roles
{
    public record RoleDeletedEvent(
    int Id
    ) : INotification;
}
