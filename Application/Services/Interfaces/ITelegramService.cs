namespace Application.Services.Interfaces
{
    public interface ITelegramService
    {
        Task ProcessTelegramUpdateAsync(string body);
    }
}
