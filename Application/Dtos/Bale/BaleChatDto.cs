using System.Text.Json.Serialization;

namespace Application.Dtos.Bale
{
    public class BaleChatDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
