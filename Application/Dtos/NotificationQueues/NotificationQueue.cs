namespace Application.Dtos.NotificationQueues
{
    public record NotificationMessage(long ChatId, string Title, decimal OldPrice, decimal NewPrice, string DigiKalaId, string? ImageFileName);
}
