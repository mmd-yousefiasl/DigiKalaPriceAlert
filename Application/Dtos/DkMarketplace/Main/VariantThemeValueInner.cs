namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantThemeValueInner
    {
        public int? Id { get; init; }
        public string? TitleFa { get; init; }
        public string? TitleEn { get; init; }
        public VariantThemeValueValue? Value { get; init; }
        public string? Nature { get; init; }
        public bool? Active { get; init; }
        public int? StandardUnitID { get; init; }
        public List<int>? ColorPallateIDs { get; init; }
        public Dictionary<string, object>? ExtraData { get; init; }
    }
}
