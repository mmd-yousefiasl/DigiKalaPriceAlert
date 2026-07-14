using Domain.Enums.Products;

namespace Application.Dtos.Products.ClientSide
{
    public class ProductWatcherDto
    {
        public int UserId { get; set; }
        public string Mobile { get; set; }
        public string? FirstName { get; set; }
        public bool IsBanned { get; set; }

        public WatchMode WatchMode { get; set; }
        public bool NotifyOnPriceChange { get; set; }
        public bool NotifyOnBuyBoxChange { get; set; }
        public decimal? MinPriceChangeThreshold { get; set; }

        public List<int> WatchedVariantIds { get; set; } = [];
    }
}
