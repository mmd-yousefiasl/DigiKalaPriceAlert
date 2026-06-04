namespace Application.Dtos.Users.BotSide
{
    public class BotSideGetUsersTrackingProductDto
    {
        public int ProductId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
