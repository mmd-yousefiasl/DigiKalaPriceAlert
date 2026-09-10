using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaVariant
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("status")]
        public string? Status { get; init; }

        [JsonPropertyName("rank")]
        public decimal Rank { get; init; }

        [JsonPropertyName("price")]
        public DigiKalaPrice? Price { get; init; }

        [JsonPropertyName("color")]
        public DigiKalaColor? Color { get; init; }

        [JsonPropertyName("seller")]
        public DigiKalaSeller? Seller { get; init; }
    }

    public sealed class DefaultVariantIdConverter : JsonConverter<long?>
    {
        public override long? Read(
            ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:
                    return reader.TryGetInt64(out var n) && n != 0 ? n : null;

                // Both object and array need real consumption, not Skip().
                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    using (var doc = JsonDocument.ParseValue(ref reader))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Object
                            && doc.RootElement.TryGetProperty("id", out var id)
                            && id.ValueKind == JsonValueKind.Number
                            && id.TryGetInt64(out var objId)
                            && objId != 0)
                        {
                            return objId;
                        }
                        return null; // [], {}, or object without usable id
                    }

                // Single-token scalars (true, false, "", etc.). No Skip() needed —
                // the serializer advances past them once we return.
                default:
                    return null;
            }
        }

        public override void Write(
            Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
            => writer.WriteNullValue();
    }
}
