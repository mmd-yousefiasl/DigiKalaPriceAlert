using Domain.Models.Users;

namespace Domain.Models.Products
{
    public class Product : AuditBaseEntity<int>
    {
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? ChangePercent { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageFileName { get; set; }

        #region Relations
        public ICollection<UserProductMapping>? Users { get; set; }
        public ICollection<ProductPriceHistory>? PriceHistory { get; set; }
        #endregion
    }
}