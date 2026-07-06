using Application.Services.Interfaces;
using Domain.Events.BlogCategories;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class BlogCategoryCacheHandlers : INotificationHandler<BlogCategoryCreatedEvent>,
        INotificationHandler<BlogCategoryUpdatedEvent>,
        INotificationHandler<BlogCategoryDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public BlogCategoryCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion
        public Task Handle(BlogCategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCategoryCacheKey);
        }

        public Task Handle(BlogCategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCategoryCacheKey);
        }

        public Task Handle(BlogCategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.BlogCategoryCacheKey);
        }
    }
}
