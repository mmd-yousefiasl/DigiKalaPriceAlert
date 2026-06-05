namespace Application.Dtos.Users.BotSide
{
    public class BotSideUserTrackingProductDto
    {
        public string Mobile { get; set; }
        public long? ChatId { get; set; }
        public bool IsPaidPlan { get; set; }
    }
}
