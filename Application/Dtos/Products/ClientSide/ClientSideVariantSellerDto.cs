namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantSellerDto : CommonDto
    {
        public string SellerName { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
        public bool IsUserTracked { get; set; }
        public bool IsActive { get; set; }
        public decimal? SellerRating { get; set; }
    }
}
