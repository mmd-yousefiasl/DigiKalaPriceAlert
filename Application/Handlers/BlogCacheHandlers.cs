using Application.Services.Interfaces;
using Domain.Events.Blogs;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class BlogCacheHandlers : INotificationHandler<BlogCreatedEvent>,
        INotificationHandler<BlogUpdatedEvent>,
        INotificationHandler<BlogDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public BlogCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public Task Handle(BlogCreatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCacheKey);
        }

        public Task Handle(BlogUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCacheKey);
        }

        public Task Handle(BlogDeletedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCacheKey);
        }
    }
}
