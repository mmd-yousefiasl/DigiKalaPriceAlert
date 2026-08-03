namespace Domain.Models.Products
{
    public class ProductPriceHistory : AuditBaseEntity<int>
    {
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? ChangePercent { get; set; }
        public bool BuyBoxChanged { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public string ChangeDescription { get; set; } = string.Empty;

        #region Relations
        public int ProductVariantSellerId { get; set; }
        public ProductVariantSeller VariantSeller { get; set; }
        #endregion
    }
}
