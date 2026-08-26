using Domain.Models.Users;

namespace Domain.Models.Products
{
    public class ProductVariantSeller : AuditBaseEntity<int>
    {
        public string DigiKalaSellerId { get; set; }
        public string SellerName { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public decimal? SellerRating { get; set; }
        public bool IsActive { get; set; }

        // Color info lives here directly (variants are flattened into sellers)
        public long DigiKalaVariantId { get; set; }
        public string ColorName { get; set; }
        public string? ColorHex { get; set; }

        #region Relations
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public ICollection<UserProductMapping> Users { get; set; } = new List<UserProductMapping>();
        #endregion
    }
}
