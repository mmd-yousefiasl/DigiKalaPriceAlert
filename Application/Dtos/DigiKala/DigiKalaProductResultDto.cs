namespace Application.Dtos.DigiKala
{
    public record DigiKalaProductResultDto
        (
        string Title,
        decimal Price,
        string? ImageUrl,
        byte[]? ImageBytes
        );
}
