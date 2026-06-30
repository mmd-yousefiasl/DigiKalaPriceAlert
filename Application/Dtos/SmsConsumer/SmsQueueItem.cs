namespace Application.Dtos.SmsConsumer
{
    public record SmsQueueItem(
    string Mobile,
    string ApiKey,
    int PatternId,
    List<SmsParameterItem> Parameters);
}
