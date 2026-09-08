using Domain.Enums.Products;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantChangeItemDto
    {
        public ChangeItemType Type { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? ChangePercent { get; set; }
    }
}
