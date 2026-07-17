namespace Domain.Models.Products
{
    public class ProductVariant : AuditBaseEntity<int>
    {
        public long DigiKalaVariantId { get; set; }
        public string ColorName { get; set; }
        public string? ColorHex { get; set; }
        public string? SizeLabel { get; set; }
        public bool IsActive { get; set; }


        #region Relations
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public ICollection<ProductVariantSeller> Sellers { get; set; } = new List<ProductVariantSeller>();
        #endregion
    }
}
