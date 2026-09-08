using Domain.Enums.Products;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantChangeDto
    {
        public DateTime CreatedDate { get; set; }
        public bool IsStockOut { get; set; }
        public List<ClientSideVariantChangeItemDto> Items { get; set; } = [];
    }
}
