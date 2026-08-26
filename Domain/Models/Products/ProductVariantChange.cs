using Domain.Enums.Products;
using Domain.Models;

namespace Domain.Models.Products
{
    /// <summary>
    /// One consolidated change record per color group per monitoring cycle.
    /// Items hold everything that happened in that cycle (price changes,
    /// buy-box flips, sellers added/removed) as a jsonb collection.
    /// NOTE: grouped by COLOR NAME, not DigiKalaVariantId — DigiKala's
    /// variant id is unique per color×seller combo, not per color.
    /// </summary>
    public class ProductVariantChange : AuditBaseEntity<int>
    {
        public int ProductId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public bool IsStockOut { get; set; }

        #region Relations
        public ICollection<ProductVariantChangeItem> Items { get; set; } = new List<ProductVariantChangeItem>();
        public Product Product { get; set; }
        #endregion
    }

    public class ProductVariantChangeItem
    {
        public ChangeItemType Type { get; set; }
        public string DigiKalaSellerId { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? ChangePercent { get; set; }
    }
}
