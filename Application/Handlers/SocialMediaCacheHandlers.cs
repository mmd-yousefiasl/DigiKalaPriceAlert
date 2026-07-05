using Application.Services.Interfaces;
using Domain.Events.SocialMedia;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class SocialMediaCacheHandlers : INotificationHandler<SocialMediaCreatedEvent>,
        INotificationHandler<SocialMediaUpdatedEvent>,
        INotificationHandler<SocialMediaDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctro
        public SocialMediaCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public Task Handle(SocialMediaCreatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SocialMediaCacheKey);
        }

        public Task Handle(SocialMediaUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SocialMediaCacheKey);
        }

        public Task Handle(SocialMediaDeletedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.SocialMediaCacheKey);
        }
    }
}
