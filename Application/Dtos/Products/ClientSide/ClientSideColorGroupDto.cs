namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideColorGroupDto
    {
        public string ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int SellersCount { get; set; }

        public ClientSideVariantSellerDto? Winner { get; set; }
        public List<ClientSideVariantSellerDto> OtherSellers { get; set; } = [];
        public List<ClientSideSellerPriceHistoryDto> RecentHistory { get; set; } = [];
    }
}
