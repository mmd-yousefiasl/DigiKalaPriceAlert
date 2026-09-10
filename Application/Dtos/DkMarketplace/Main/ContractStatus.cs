namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class ContractStatus
    {
        public bool? Signed { get; init; }
        public bool? Expired { get; init; }
        public bool? Restricted { get; init; }
    }
}
