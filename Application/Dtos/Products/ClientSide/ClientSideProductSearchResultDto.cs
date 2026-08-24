namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductSearchResultDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<ClientSideProductOptionDto> Options { get; set; } = new();
    }

    public class ClientSideProductOptionDto
    {
        public int VariantId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public List<ClientSideProductSellerOptionDto> Sellers { get; set; } = new();
    }

    public class ClientSideProductSellerOptionDto
    {
        public int SellerId { get; set; } // The DB ID we need for the mapping table later
        public long DigiKalaVariantId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsBuyBoxWinner { get; set; }
    }
}
