namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class UpdateVariantStockData
    {
        public int? SellingStock { get; init; }
        public int? MarketplaceSellerStock { get; init; }
        public int? DkWarehouseStock { get; init; }
        public int? DigikalaReservation { get; init; }
        public int? SellerReservation { get; init; }
    }
}
