using Domain.Enums.Products;
using MediatR;

namespace Domain.Events.Products
{
    /// <summary>
    /// One event per affected COLOR GROUP (identified by ProductId + ColorName).
    /// DigiKala's variant id is unique per color×seller combo, so the color
    /// name is the correct grouping key for watchers and change records.
    /// </summary>
    public record ProductVariantMarketStatusUpdatedEvent(
        int ProductId,
        string ProductTitle,
        string ProductUrl,
        string VariantName,
        IReadOnlyList<VariantChangeItemInfo> Changes,
        IReadOnlyList<SellerMarketStatus> Sellers,
        bool IsAvailable
    ) : INotification;

    public record SellerMarketStatus(
        string DigiKalaSellerId,
        string SellerName,
        decimal CurrentPrice,
        bool IsBuyBoxWinner
    );

    public record VariantChangeItemInfo(
        ChangeItemType Type,
        string DigiKalaSellerId,
        string SellerName,
        decimal OldPrice,
        decimal NewPrice,
        decimal? ChangePercent
    );
}
