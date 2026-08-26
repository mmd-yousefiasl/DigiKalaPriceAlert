using Application.Services.Interfaces;
using Domain.Events.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers
{
    /// <summary>
    /// Thin MediatR bridge: forwards variant market events to the alert
    /// service. The try/catch guarantees alerting problems never break the
    /// monitoring cycle.
    /// </summary>
    public class MarketStatusAlertHandler : INotificationHandler<ProductVariantMarketStatusUpdatedEvent>
    {
        private readonly IMarketAlertService _marketAlertService;
        private readonly ILogger<MarketStatusAlertHandler> _logger;

        public MarketStatusAlertHandler(
            IMarketAlertService marketAlertService,
            ILogger<MarketStatusAlertHandler> logger)
        {
            _marketAlertService = marketAlertService;
            _logger = logger;
        }

        public async Task Handle(
            ProductVariantMarketStatusUpdatedEvent notification,
            CancellationToken cancellationToken)
        {
            try
            {
                await _marketAlertService.DispatchMarketStatusAlertAsync(notification, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Alert dispatch failed for product {ProductId} color {ColorName}",
                    notification.ProductId, notification.VariantName);
            }
        }
    }
}
