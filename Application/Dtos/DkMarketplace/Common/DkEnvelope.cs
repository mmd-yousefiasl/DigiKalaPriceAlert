namespace Application.Dtos.DkMarketplace.Common
{
    internal sealed class DkEnvelope<T> where T : class
    {
        public string? Status { get; init; }
        public T? Data { get; init; }
    }
}
