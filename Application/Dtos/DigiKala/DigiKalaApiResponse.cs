using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaApiResponse
    {
        [JsonPropertyName("data")]
        public DigiKalaData? Data { get; init; }
    }
}
