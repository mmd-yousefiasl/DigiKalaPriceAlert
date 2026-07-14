using Domain.Models.Users;

namespace Domain.Models.Products
{
    public class UserVariantWatch : AuditBaseEntity<int>
    {
        public string? UserOwnSellerIdOnVariant { get; set; }

        #region Relations
        public int UserProductMappingId { get; set; }
        public UserProductMapping ProductMapping { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant Variant { get; set; }
        #endregion
    }
}
