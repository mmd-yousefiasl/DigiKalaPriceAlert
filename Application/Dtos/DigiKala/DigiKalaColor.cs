using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaColor
    {
        [JsonPropertyName("id")]
        public int? Id { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("hex_code")]
        public string? HexCode { get; init; }
    }
}
