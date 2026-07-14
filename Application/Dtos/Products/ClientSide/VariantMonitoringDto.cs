namespace Application.Dtos.Products.ClientSide
{
    public class VariantMonitoringDto : CommonDto
    {
        public long DigiKalaVariantId { get; set; }
        public string ColorName { get; set; }
        public bool IsActive { get; set; }
        public List<SellerMonitoringDto> Sellers { get; set; } = [];
        public List<ProductWatcherDto> Watchers { get; set; } = [];
    }
}
