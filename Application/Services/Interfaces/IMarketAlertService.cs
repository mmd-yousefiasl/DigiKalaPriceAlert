using Domain.Events.Products;

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Dispatches market-status alerts (Telegram) for a variant change event.
    /// Must NEVER let exceptions escape into the monitoring cycle.
    /// </summary>
    public interface IMarketAlertService
    {
        Task DispatchMarketStatusAlertAsync(
            ProductVariantMarketStatusUpdatedEvent notification,
            CancellationToken cancellationToken = default);
    }
}
