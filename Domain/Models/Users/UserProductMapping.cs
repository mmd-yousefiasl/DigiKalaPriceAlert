using Domain.Enums.Products;
using Domain.Models.Products;

namespace Domain.Models.Users
{
    public class UserProductMapping : AuditBaseEntity<int>
    {
        public bool IsRead { get; set; } = false;
        public WatchMode WatchMode { get; set; } = WatchMode.AllVariants;

        public bool NotifyOnPriceChange { get; set; } = true;
        public bool NotifyOnBuyBoxChange { get; set; } = true;
        public bool NotifyOnNewSeller { get; set; } = false;
        public bool NotifyOnSellerLeft { get; set; } = false;
        public decimal? MinPriceChangeThreshold { get; set; }

        #region Relations
        public int UserId { get; set; }
        public User? User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public ICollection<UserVariantWatch> VariantWatches { get; set; }
        #endregion
    }
}
