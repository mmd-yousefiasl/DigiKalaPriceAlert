namespace Application.Dtos.Products.ClientSide
{
    public record TrackedProductDto(
        int ProductId,
        string DigiKalaId,
        string Title,
        List<TrackedSellerDto> Sellers);
}
