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

        #region Relations
        public int ProductVariantId { get; set; }
        public ProductVariant Variant { get; set; }

        public ICollection<ProductPriceHistory> PriceHistory { get; set; } = new List<ProductPriceHistory>();
        #endregion
    }
}
