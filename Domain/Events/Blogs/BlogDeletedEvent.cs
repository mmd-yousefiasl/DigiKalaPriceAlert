using MediatR;

namespace Domain.Events.Blogs
{
    public record BlogDeletedEvent(
        int Id
        ) : INotification;
}
