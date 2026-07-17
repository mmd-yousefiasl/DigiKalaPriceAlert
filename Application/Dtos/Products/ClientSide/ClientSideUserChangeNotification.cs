namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideUserChangeNotification
    {
        public int UserId { get; set; }
        public bool HasPriceChange { get; set; }
        public bool HasBuyBoxChange { get; set; }
    }
}
