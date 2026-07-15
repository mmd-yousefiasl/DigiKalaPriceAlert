namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDetailsDto : CommonDto
    {
        public string Title { get; set; }
        public string DigiKalaId { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public decimal LowestCurrentPrice { get; set; }
        public decimal HighestCurrentPrice { get; set; }
        public int TotalVariantsCount { get; set; }
        public int TotalSellersCount { get; set; }

        public List<ClientSideColorGroupDto> ColorGroups { get; set; } = [];
    }
}
