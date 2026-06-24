namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideGetProductsBatchForMonitoringDto
    {
        public List<int> ProductIds { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
