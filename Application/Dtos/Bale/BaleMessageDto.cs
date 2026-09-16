using System.Text.Json.Serialization;

namespace Application.Dtos.Bale
{
    public class BaleMessageDto
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }

        [JsonPropertyName("from")]
        public BaleUserDto? From { get; set; }

        [JsonPropertyName("chat")]
        public BaleChatDto Chat { get; set; } = new();

        [JsonPropertyName("date")]
        public long Date { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
