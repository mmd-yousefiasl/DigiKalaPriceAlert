namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantStockData
    {
        public int? MarketplaceSellerStock { get; init; }
        public int? WarehouseStock { get; init; }
        public int? OnTheWayStock { get; init; }
        public int? Reservation { get; init; }
        public int? LeftConsumer { get; init; }
        public int? SellerReservation { get; init; }
        public int? DigikalaReservation { get; init; }
    }
}
