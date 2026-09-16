using System.Text.Json.Serialization;

namespace Application.Dtos.Bale
{
    public class BaleUpdateDto
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }

        [JsonPropertyName("message")]
        public BaleMessageDto? Message { get; set; }

        [JsonPropertyName("callback_query")]
        public BaleCallbackQueryDto? CallbackQuery { get; set; }
    }
}
