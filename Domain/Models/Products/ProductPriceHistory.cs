namespace Domain.Models.Products
{
    public class ProductPriceHistory : AuditBaseEntity<int>
    {
        public decimal Price { get; set; }

        #region Relations
        public int ProductId { get; set; }
        public Product Product { get; set; }
        #endregion
    }
}
