using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaImages
    {
        [JsonPropertyName("main")]
        public DigiKalaImage? Main { get; init; }
    }
}
