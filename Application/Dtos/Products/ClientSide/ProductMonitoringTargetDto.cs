namespace Application.Dtos.Products.ClientSide
{
    public class ProductMonitoringTargetDto : CommonDto
    {
        public int ProductId { get; set; }
        public string DigiKalaId { get; set; }
        public List<ProductWatcherDto> Watchers { get; set; } = [];
    }
}