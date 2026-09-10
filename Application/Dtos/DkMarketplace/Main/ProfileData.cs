namespace Application.Dtos.DkMarketplace.Main
{
    public sealed class ProfileData
    {
        public int? SellerId { get; init; }
        public string? HashedSellerId { get; init; }
        public int? WalletCredit { get; init; }
        public string? SellerName { get; init; }
        public string? SecondaryBusinessName { get; init; }
        public double? SellerRating { get; init; }
        public bool? IsPrivate { get; init; }
        public int? NewNotifications { get; init; }
        public int? NewQuestions { get; init; }
        public SellerVerification? Verification { get; init; }
        public string? RegistrationStatus { get; init; }
        public List<string>? RejectionReasons { get; init; }
        public string? RejectionReasonsExtraMessage { get; init; }
        public string? LogoRejectionReason { get; init; }
        public string? DescriptionRejectionReason { get; init; }
        public RegisterPhone? RegisterPhone { get; init; }
        public ContractStatus? ContractStatus { get; init; }
        public bool? HasPasswordExpired { get; init; }
        public bool? IsShipmentListVisible { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Gender { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
        public string? State { get; init; }
        public string? City { get; init; }
        public string? Birthday { get; init; }
        public string? Company { get; init; }
        public bool? IsNewSeller { get; init; }
        public string? NationalId { get; init; }
        public RegistrationMetadata? RegistrationMetadata { get; init; }
        public SellerMetadata? Metadata { get; init; }
    }
}
