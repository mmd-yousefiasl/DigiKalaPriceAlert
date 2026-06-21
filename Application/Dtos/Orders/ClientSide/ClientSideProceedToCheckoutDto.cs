namespace Application.Dtos.Orders.ClientSide
{
    public class ClientSideProceedToCheckoutDto
    {
        public bool IsDone { get; set; }
        public string? TransId { get; set; }
    }
}
