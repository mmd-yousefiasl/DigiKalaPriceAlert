namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideVariantDetailsDto : CommonDto
    {
        public long DigiKalaVariantId { get; set; }
        public string ColorName { get; set; }
        public string? ColorHex { get; set; }
        public bool IsActive { get; set; }

        public ClientSideVariantSellerDto Seller { get; set; }
    }
}
