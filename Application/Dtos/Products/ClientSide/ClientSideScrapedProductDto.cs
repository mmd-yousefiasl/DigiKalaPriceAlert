namespace Application.Dtos.Products.ClientSide
{
    public record ClientSideScrapedProductDto
    {
        public string DigiKalaId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public long DefaultVariantId { get; set; }

        public List<ClientSideScrapedSellerDto> Sellers { get; set; } = new();
    }
}