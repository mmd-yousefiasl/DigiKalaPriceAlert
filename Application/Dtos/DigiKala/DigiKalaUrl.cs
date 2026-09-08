using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaUrl
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; init; }
    }
}
