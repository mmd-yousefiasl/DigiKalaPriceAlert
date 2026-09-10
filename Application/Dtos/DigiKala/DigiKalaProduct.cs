using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaProduct
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title_fa")]
        public string? Title { get; init; }

        [JsonPropertyName("url")]
        public DigiKalaUrl? Url { get; init; }

        [JsonPropertyName("images")]
        public DigiKalaImages? Images { get; init; }

        [JsonPropertyName("default_variant")]
        [JsonConverter(typeof(DefaultVariantIdConverter))]
        public long? DefaultVariantId { get; init; }

        [JsonPropertyName("variants")]
        public List<DigiKalaVariant>? Variants { get; init; }
    }
}
