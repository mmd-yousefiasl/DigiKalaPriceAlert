namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideScrapedSellerDto
    {
        public long DigiKalaVariantId { get; set; }
        public int? ColorId { get; set; }
        public string ColorName { get; set; } = "";
        public string? ColorHex { get; set; }
        public long SellerId { get; set; }
        public string SellerName { get; set; } = "";
        public string SellerCode { get; set; } = "";
        public decimal? SellerRating { get; set; }
        public long Price { get; set; }
        public decimal? Rank { get; set; }
        public string Status { get; set; } = "";
        public bool IsBuyBoxWinner { get; set; }
    }
}
