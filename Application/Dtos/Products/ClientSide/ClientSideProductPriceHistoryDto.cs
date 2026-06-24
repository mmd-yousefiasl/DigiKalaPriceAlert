namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductPriceHistoryDto : CommonDto
    {
        public DateTime CreatedDate { get; set; }
        public decimal Price { get; set; }
    }
}
