namespace Application.Dtos.Products.BotSide
{
    public class BotSideGetProductsBatchForMonitoringDto
    {
        public List<int> ProductIds { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
