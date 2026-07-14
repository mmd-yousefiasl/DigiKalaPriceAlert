using Domain.Models.Users;

namespace Domain.Models.Products
{
    public class Product : AuditBaseEntity<int>
    {
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }

        #region Relations
        public ICollection<UserProductMapping> Users { get; set; } = new List<UserProductMapping>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        #endregion
    }
}