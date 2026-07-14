namespace Application.Dtos.Products.ClientSide
{
    public record ClientSideScrapedVariantDto
    {
        public long DigiKalaVariantId { get; init; }
        public string Status { get; init; } = string.Empty;
        public bool IsMarketable => Status == "marketable";
        public decimal Rank { get; init; }
        public bool IsBuyBoxWinner { get; set; }

        public int? ColorId { get; init; }
        public string ColorName { get; init; } = "بدون رنگ";
        public string? ColorHex { get; init; }

        public long SellerId { get; init; }
        public string SellerName { get; init; } = string.Empty;
        public string SellerCode { get; init; } = string.Empty;
        public decimal? SellerRating { get; init; }

        public decimal Price { get; init; }
    }
}
