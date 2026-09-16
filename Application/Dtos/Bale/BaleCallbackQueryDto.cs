using System.Text.Json.Serialization;

namespace Application.Dtos.Bale
{
    public class BaleCallbackQueryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public BaleUserDto? From { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}
