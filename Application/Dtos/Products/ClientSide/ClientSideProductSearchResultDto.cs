namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductSearchResultDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public List<ClientSideProductOptionDto> Options { get; set; } = new();
    }
}
