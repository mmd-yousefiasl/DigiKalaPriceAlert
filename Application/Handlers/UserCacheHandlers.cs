using Application.Services.Interfaces;
using Domain.Events.Users;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class UserCacheHandlers : INotificationHandler<UserUpdatedEvent>,
        INotificationHandler<UserDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public UserCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
        {
            string key1 = string.Format(CacheKeys.IsSuperAdminCacheKey, notification.Id);
            string key2 = string.Format(CacheKeys.UserRoleIdsCacheKey, notification.Id);

            await Task.WhenAll(
                _cacheManager.RemoveByKeyAsync(key1),
                _cacheManager.RemoveByKeyAsync(key2)
            );
        }

        public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            string key1 = string.Format(CacheKeys.IsSuperAdminCacheKey, notification.Id);
            string key2 = string.Format(CacheKeys.UserRoleIdsCacheKey, notification.Id);

            await Task.WhenAll(
                _cacheManager.RemoveByKeyAsync(key1),
                _cacheManager.RemoveByKeyAsync(key2)
            );
        }
    }
}
