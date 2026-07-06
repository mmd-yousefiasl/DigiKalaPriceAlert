using Domain.Enums.GeneralInfo;
using MediatR;

namespace Domain.Events.SiteMap
{
    public record SiteMapCreatedEvent(
    string Url,
    ChangeFreqEnum ChangeFreq,
    decimal Priority,
    bool IsPublished,
    SiteMapTypeEnum Type
    ) : INotification;
}
