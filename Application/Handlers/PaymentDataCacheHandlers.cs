using Application.Services.Interfaces;
using Domain.Events.PaymentData;
using Domain.Statics;
using MediatR;

namespace Application.Handlers
{
    public class PaymentDataCacheHandlers : INotificationHandler<PaymentDataUpdatedEvent>
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public PaymentDataCacheHandlers(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }
        #endregion

        public Task Handle(PaymentDataUpdatedEvent notification, CancellationToken cancellationToken)
        {
            return _cacheManager.RemoveByKeyAsync(CacheKeys.PaymentDataCacheKey);
        }
    }
}
