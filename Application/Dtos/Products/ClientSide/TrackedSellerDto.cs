namespace Application.Dtos.Products.ClientSide
{
    public record TrackedSellerDto(
        int SellerDbId,
        long DigiKalaVariantId,
        string DigiKalaSellerId,
        decimal CurrentPrice,
        bool IsBuyBoxWinner,
        bool WasBuyBoxWinner);
}
