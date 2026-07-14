namespace Application.Dtos.Products.ClientSide
{
    public record ClientSideScrapedProductDto
    {
        public string DigiKalaId { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public long DefaultVariantId { get; init; }
        public List<ClientSideScrapedVariantDto> Variants { get; init; } = [];
    }
}
