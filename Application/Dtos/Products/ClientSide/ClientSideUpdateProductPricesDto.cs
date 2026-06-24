namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideUpdateProductPricesDto
    {
        public int ProductId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
