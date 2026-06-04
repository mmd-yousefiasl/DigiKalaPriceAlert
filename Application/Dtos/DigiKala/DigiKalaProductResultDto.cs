namespace Application.Dtos.DigiKala
{
    public record DigiKalaProductResultDto
        (
        string Title,
        decimal Price,
        byte[]? ImageBytes
        );
}
