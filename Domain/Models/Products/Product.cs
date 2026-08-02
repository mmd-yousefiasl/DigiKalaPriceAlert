namespace Domain.Models.Products
{
    public class Product : AuditBaseEntity<int>
    {
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }

        #region Relations
        public ICollection<ProductVariantSeller> Sellers { get; set; } = new List<ProductVariantSeller>();
        #endregion
    }
}