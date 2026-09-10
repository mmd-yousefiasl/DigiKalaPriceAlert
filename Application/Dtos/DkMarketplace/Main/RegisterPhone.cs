namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class RegisterPhone
    {
        public bool? Submitted { get; init; }
        public string? SeenWarningAt { get; init; }
        public bool? Restricted { get; init; }
    }
}
