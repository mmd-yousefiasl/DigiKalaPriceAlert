using Application.Services.Interfaces;
using Domain.Events.SiteMap;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class SiteMapCacheHandlers : INotificationHandler<SiteMapCreatedEvent>,
        INotificationHandler<SiteMapUpdatedEvent>,
        INotificationHandler<SiteMapDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public SiteMapCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public Task Handle(SiteMapCreatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SiteMapCacheKey);
        }

        public Task Handle(SiteMapUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SiteMapCacheKey);
        }

        public Task Handle(SiteMapDeletedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SiteMapCacheKey);
        }
    }
}
