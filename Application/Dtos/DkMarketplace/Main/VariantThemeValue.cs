namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantThemeValue
    {
        public int? ThemeId { get; init; }
        public string? ThemeLabel { get; init; }
        public bool? Active { get; init; }
        public string? ThemeType { get; init; }
        public VariantThemeValueInner? ThemeValue { get; init; }
    }
}
