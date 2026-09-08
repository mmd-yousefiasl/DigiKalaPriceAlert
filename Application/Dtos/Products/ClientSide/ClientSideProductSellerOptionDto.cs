namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductSellerOptionDto
    {
        public int SellerId { get; set; } // The DB ID we need for the mapping table later
        public long DigiKalaVariantId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsBuyBoxWinner { get; set; }
    }
}
