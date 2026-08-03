namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideSellerPriceHistoryDto
    {
        public string SellerName { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? ChangePercent { get; set; }
        public bool BuyBoxChanged { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ChangeDescription { get; set; }
    }
}
