namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class VariantShippingOptions
    {
        public bool? IsFbsAbilityEnable { get; init; }
        public bool? IsFbdActive { get; init; }
        public bool? IsFbsActive { get; init; }
        public bool? IsNeededFbsSetting { get; init; }
        public bool? IsSbsModuleActive { get; init; }
        public bool? OnlySbs { get; init; }
        public bool? IsThreeHourDeliveryActive { get; init; }
        public bool? IsThreeHourCheckboxActive { get; init; }
    }
}
