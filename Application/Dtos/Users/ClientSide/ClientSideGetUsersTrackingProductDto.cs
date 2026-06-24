namespace Application.Dtos.Users.ClientSide
{
    public class ClientSideGetUsersTrackingProductDto
    {
        public int ProductId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
