using System.Text.Json.Serialization;

namespace Application.Dtos.DigiKala
{
    public sealed class DigiKalaApiResponse
    {
        [JsonPropertyName("data")]
        public DigiKalaData? Data { get; init; }
    }

    public sealed class DigiKalaData
    {
        [JsonPropertyName("product")]
        public DigiKalaProduct? Product { get; init; }
    }

    public sealed class DigiKalaProduct
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title_fa")]
        public string? Title { get; init; }

        [JsonPropertyName("url")]
        public DigiKalaUrl? Url { get; init; }

        [JsonPropertyName("images")]
        public DigiKalaImages? Images { get; init; }

        [JsonPropertyName("default_variant")]
        public DigiKalaVariant? DefaultVariant { get; init; }

        [JsonPropertyName("variants")]
        public List<DigiKalaVariant>? Variants { get; init; }
    }

    public sealed class DigiKalaUrl
    {
        [JsonPropertyName("uri")]
        public string? Uri { get; init; }
    }

    public sealed class DigiKalaImages
    {
        [JsonPropertyName("main")]
        public DigiKalaImage? Main { get; init; }
    }

    public sealed class DigiKalaImage
    {
        [JsonPropertyName("url")]
        public List<string>? Urls { get; init; }
    }

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

    public sealed class DigiKalaPrice
    {
        [JsonPropertyName("selling_price")]
        public long SellingPrice { get; init; }
    }

    public sealed class DigiKalaColor
    {
        [JsonPropertyName("id")]
        public int? Id { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("hex_code")]
        public string? HexCode { get; init; }
    }

    public sealed class DigiKalaSeller
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("code")]
        public string? Code { get; init; }

        [JsonPropertyName("rating")]
        public DigiKalaSellerRating? Rating { get; init; }
    }

    public sealed class DigiKalaSellerRating
    {
        [JsonPropertyName("total_rate")]
        public decimal? TotalRate { get; init; }
    }
}
