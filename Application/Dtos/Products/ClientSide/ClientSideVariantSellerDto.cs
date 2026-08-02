namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantSellerDto : CommonDto
    {
        public string SellerName { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public bool IsUserTracked { get; set; }
        public bool IsActive { get; set; }
        public decimal? SellerRating { get; set; }

        // Computed properties for UI
        public decimal? PriceChange => PreviousPrice.HasValue ? CurrentPrice - PreviousPrice : null;
        public decimal? PriceChangePercent => (PreviousPrice.HasValue && PreviousPrice > 0) ? ((CurrentPrice - PreviousPrice.Value) / PreviousPrice.Value) * 100 : null;
    }
}
