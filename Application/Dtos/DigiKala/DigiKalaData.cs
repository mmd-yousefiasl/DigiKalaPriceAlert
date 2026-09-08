using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaData
    {
        [JsonPropertyName("product")]
        public DigiKalaProduct? Product { get; init; }
    }
}
