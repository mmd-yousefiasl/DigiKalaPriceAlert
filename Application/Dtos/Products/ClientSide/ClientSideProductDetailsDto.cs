namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDetailsDto : CommonDto
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string DigiKalaId { get; set; }
        public string? ImageUrl { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public string VariantColorName { get; set; }
        public string? VariantColorHex { get; set; }
        public int TotalActiveSellersInVariant { get; set; }

        public decimal LowestCurrentPrice { get; set; }
        public decimal HighestCurrentPrice { get; set; }

        public ClientSideVariantSellerDto? TrackedSeller { get; set; }
        public ClientSideVariantSellerDto? WinnerSeller { get; set; }
        public List<ClientSideVariantSellerDto> Opponents { get; set; } = [];

        public List<ClientSideSellerPriceHistoryDto> RecentHistory { get; set; } = [];
    }
}
