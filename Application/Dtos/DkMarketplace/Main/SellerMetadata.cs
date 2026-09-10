namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class SellerMetadata
    {
        public string? Tier { get; init; }
        public string? RegisterStatus { get; init; }
        public string? Status { get; init; }
        public List<int>? SupplyCategories { get; init; }
        public string? OpenApiTokenStatus { get; init; }
        public bool? HasSbsSetting { get; init; }
        public bool? HasNearbySetting { get; init; }
        public int? CityId { get; init; }
        public string? Platform { get; init; }
    }
}
