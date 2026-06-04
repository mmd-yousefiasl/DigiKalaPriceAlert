namespace Application.Dtos.Products.BotSide
{
    public class BotSideGetProductsBatchForMonitoringDto
    {
        public int Page { get; set; }
        public int BarchSize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
