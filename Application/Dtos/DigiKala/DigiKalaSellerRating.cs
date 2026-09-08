using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaSellerRating
    {
        [JsonPropertyName("total_rate")]
        public decimal? TotalRate { get; init; }
    }
}
