using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaVariant
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("rank")]
        public decimal Rank { get; init; }

        [JsonPropertyName("price")]
        public DigiKalaPrice? Price { get; init; }

        [JsonPropertyName("color")]
        public DigiKalaColor? Color { get; init; }

        [JsonPropertyName("seller")]
        public DigiKalaSeller? Seller { get; init; }
    }
}
