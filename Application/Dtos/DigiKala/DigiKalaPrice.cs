using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaPrice
    {
        [JsonPropertyName("selling_price")]
        public long SellingPrice { get; init; }
    }
}
