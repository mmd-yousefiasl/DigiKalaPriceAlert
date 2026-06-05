namespace Application.Dtos.Users.BotSide
{
    public class BotSideUserMonitoringDto
    {
        public long? ChatId { get; set; }
        public List<BotSideUserProductDetailsDto> AllowedProducts { get; set; }
    }
}
