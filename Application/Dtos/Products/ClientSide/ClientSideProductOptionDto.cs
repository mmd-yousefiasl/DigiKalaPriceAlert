namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductOptionDto
    {
        public int VariantId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public List<ClientSideProductSellerOptionDto> Sellers { get; set; } = new();
    }
}
