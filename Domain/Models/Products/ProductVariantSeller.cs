using Domain.Models.Users;

namespace Domain.Models.Products
{
    public class ProductVariantSeller : AuditBaseEntity<int>
    {
        public string DigiKalaSellerId { get; set; }
        public string SellerName { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal? PreviousPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public bool WasBuyBoxWinner { get; set; }
        public decimal? SellerRating { get; set; }
        public bool IsActive { get; set; }

        // NEW: Color info moved here directly!
        public long DigiKalaVariantId { get; set; }
        public string ColorName { get; set; }
        public string? ColorHex { get; set; }

        #region Relations
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public ICollection<ProductPriceHistory> PriceHistory { get; set; } = new List<ProductPriceHistory>();
        public ICollection<UserProductMapping> Users { get; set; } = new List<UserProductMapping>();
        #endregion
    }
}
