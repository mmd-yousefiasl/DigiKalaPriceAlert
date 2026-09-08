using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaImage
    {
        [JsonPropertyName("url")]
        public List<string>? Urls { get; init; }
    }
}
