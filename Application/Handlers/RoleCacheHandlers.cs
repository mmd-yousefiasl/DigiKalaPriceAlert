using Application.Services.Interfaces;
using Domain.Events.Roles;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class RoleCacheHandlers : INotificationHandler<RoleCreatedEvent>,
        INotificationHandler<RoleUpdatedEvent>,
        INotificationHandler<RoleDeletedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public RoleCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public Task Handle(RoleCreatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.PermissionsRoleIdsCacheKey);
        }

        public Task Handle(RoleUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.PermissionsRoleIdsCacheKey);
        }

        public Task Handle(RoleDeletedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.PermissionsRoleIdsCacheKey);
        }
    }
}
