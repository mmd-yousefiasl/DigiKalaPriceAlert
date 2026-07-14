namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantSellerDto : CommonDto
    {
        public string SellerName { get; set; }
        public string DigiKalaSellerId { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public decimal? SellerRating { get; set; }
        public bool IsActive { get; set; }

        public decimal? PriceChange => PreviousPrice.HasValue
            ? CurrentPrice - PreviousPrice.Value
            : null;

        public decimal? PriceChangePercent => PreviousPrice.HasValue && PreviousPrice.Value > 0
            ? Math.Round((CurrentPrice - PreviousPrice.Value) / PreviousPrice.Value * 100, 2)
            : null;

        public List<ClientSideSellerPriceHistoryDto> RecentHistory { get; set; } = [];
    }
}
