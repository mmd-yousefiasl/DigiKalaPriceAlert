using MediatR;

namespace Domain.Events.Products
{
    public record ProductPriceChangedEvent(
    int ProductId,
    string ProductTitle,
    int SellerId,
    long DigiKalaVariantId,
    string SellerName,
    string ColorName,
    decimal? PreviousPrice,
    decimal CurrentPrice,
    bool PriceChanged,
    bool BuyBoxWinnerChanged,
    bool IsNowBuyBoxWinner
    ) : INotification;
}
