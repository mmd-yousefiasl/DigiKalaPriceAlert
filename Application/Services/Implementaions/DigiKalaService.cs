using Application.Dtos.DigiKala;
using Application.Services.Interfaces;
using Application.Tools;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Services.Implementaions
{
    public class DigiKalaService : IDigiKalaService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly ILogger<DigiKalaService> _logger;
        #endregion

        #region Ctor
        public DigiKalaService(HttpClient httpClient, ILogger<DigiKalaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
            _httpClient.DefaultRequestHeaders.Add("X-Web-Client", "MobileWeb");
            _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        }
        #endregion

        #region Private Methods
        private async Task<Result<DigiKalaProductResultDto>> FetchProductDetailsInternalAsync(string digiKalaId, bool downloadImage)
        {
            try
            {
                string apiUrl = $"https://api.digikala.com/v2/product/{digiKalaId}/";

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if ((int)response.StatusCode == 307 || (int)response.StatusCode == 302 || (int)response.StatusCode == 301)
                {
                    var redirectUrl = response.Headers.Location?.ToString();
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        _logger.LogInformation("Following 307 Redirect to: {RedirectUrl}", redirectUrl);
                        request = new HttpRequestMessage(HttpMethod.Get, redirectUrl);
                        response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    }
                }

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(responseBody);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("data", out JsonElement dataElement) ||
                    !dataElement.TryGetProperty("product", out JsonElement productElement))
                {
                    return Result.Failure<DigiKalaProductResultDto>(AppStatics.ProductNotFoundError);
                }

                string title = productElement.TryGetProperty("title_fa", out var titleElement)
                    ? titleElement.GetString() ?? "بدون عنوان"
                    : "بدون عنوان";

                decimal price = 0;
                string? imageUrl = null;

                if (productElement.TryGetProperty("default_variant", out JsonElement defaultVariant) &&
                    defaultVariant.ValueKind == JsonValueKind.Object)
                {
                    if (defaultVariant.TryGetProperty("price", out JsonElement priceElement) &&
                        priceElement.ValueKind == JsonValueKind.Object &&
                        priceElement.TryGetProperty("selling_price", out JsonElement sellingPriceElement))
                    {
                        price = sellingPriceElement.GetDecimal() / 10;
                    }
                }
                else
                {
                    _logger.LogInformation("Product DKP: {DkpId} has no default variant. It might be out of stock.", digiKalaId);
                }

                byte[]? imageBytes = null;
                if (downloadImage)
                {
                    if (productElement.TryGetProperty("images", out JsonElement imagesElement) &&
                        imagesElement.TryGetProperty("main", out JsonElement mainImageElement) &&
                        mainImageElement.TryGetProperty("url", out JsonElement urlArrayElement) &&
                        urlArrayElement.ValueKind == JsonValueKind.Array && urlArrayElement.GetArrayLength() > 0)
                    {
                        imageUrl = urlArrayElement[0].GetString();
                    }

                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        try
                        {
                            imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to download image for DKP: {DkpId}", digiKalaId);
                        }
                    }
                }

                var dto = new DigiKalaProductResultDto(title, price, imageUrl, imageBytes);
                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product details from DigiKala for DKP: {DkpId}", digiKalaId);
            }

            return Result.Failure<DigiKalaProductResultDto>(AppStatics.ProductNotFoundError);
        }
        #endregion

        public Result<string> ExtractProductIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure<string>(AppStatics.InvalidLinkError);

            var match = Regex.Match(url, @"dkp-(\d+)", RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[1].Value;

            if (long.TryParse(url.Trim(), out _)) return url.Trim();

            return Result.Failure<string>(AppStatics.InvalidLinkError);
        }

        public async Task<Result<DigiKalaProductResultDto>> GetProductDetailsAsync(string digiKalaId)
        {
            return await FetchProductDetailsInternalAsync(digiKalaId, downloadImage: true);
        }

        public async Task<Result<DigiKalaProductResultDto>> GetProductDetailsWithoutImageAsync(string digiKalaId)
        {
            return await FetchProductDetailsInternalAsync(digiKalaId, downloadImage: false);
        }
    }
}
