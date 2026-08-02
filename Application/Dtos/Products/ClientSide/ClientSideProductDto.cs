namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDto : CommonDto
    {
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool IsRead { get; set; }
        public string UserSellerCode { get; set; }

        // Overall product stats (Keep for context)
        public int VariantsCount { get; set; }
        public int SellersCount { get; set; }

        // NEW: Seller-specific stats (The real value)
        public decimal YourPrice { get; set; }
        public bool IsWinningBuyBox { get; set; }
        public decimal CompetitorLowestPrice { get; set; }
        public bool IsUndercut { get; set; } // Is a competitor cheaper than you?
    }
}
