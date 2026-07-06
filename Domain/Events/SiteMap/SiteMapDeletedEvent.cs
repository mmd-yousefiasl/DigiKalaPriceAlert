using MediatR;

namespace Domain.Events.SiteMap
{
    public record SiteMapDeletedEvent(
    int Id
    ) : INotification;
}
