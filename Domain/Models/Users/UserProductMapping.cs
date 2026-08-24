using Domain.Models.Products;

namespace Domain.Models.Users
{
    public class UserProductMapping : AuditBaseEntity<int>
    {
        public bool IsRead { get; set; } = false;
        public long DigiKalaVariantId { get; set; }

        #region Relations
        public int UserId { get; set; }
        public User? User { get; set; }
        public int VariantSellerId { get; set; }
        public ProductVariantSeller VariantSeller { get; set; }
        #endregion
    }
}
