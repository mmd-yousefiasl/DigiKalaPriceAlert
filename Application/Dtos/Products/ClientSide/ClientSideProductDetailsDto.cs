namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDetailsDto : CommonDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public string DigiKalaId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }

        public decimal? AllTimeMinPrice { get; set; }
        public decimal? AllTimeMaxPrice { get; set; }
        public int TodayPriceChangesCount { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public List<ClientSideProductPriceHistoryDto>? History { get; set; }
    }
}
