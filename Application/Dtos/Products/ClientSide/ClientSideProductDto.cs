namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDto : CommonDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool IsRead { get; set; }

        // NEW: UX Improvements (Showing exactly what they track)
        public string SellerName { get; set; }
        public string? VariantColorName { get; set; }
        public int VariantSellerId { get; set; } // Useful for future untrack actions

        // Overall product stats (Keep for context)
        public int VariantsCount { get; set; }
        public int SellersCount { get; set; }

        // Seller-specific stats (The real value)
        public decimal YourPrice { get; set; }
        public bool IsWinningBuyBox { get; set; }
        public decimal CompetitorLowestPrice { get; set; }
        public bool IsUndercut { get; set; }
    }
}
