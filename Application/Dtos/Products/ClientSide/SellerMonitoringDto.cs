namespace Application.Dtos.Products.ClientSide
{
    public class SellerMonitoringDto : CommonDto
    {
        public string DigiKalaSellerId { get; set; }
        public decimal CurrentPrice { get; set; }
        public bool IsBuyBoxWinner { get; set; }
    }
}
