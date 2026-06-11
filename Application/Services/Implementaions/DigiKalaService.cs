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
                // آدرس اختصاصی لایه ورکر کلودفلر شما
                string workerUrl = "https://flat-sunset-4db4.mmdyousefi95.workers.dev/";

                // ۱. اضافه کردن پارامتر دانلود تصویر به آدرس ورکر کلودفلر
                string apiUrl = $"{workerUrl}?id={digiKalaId}&download_image={downloadImage.ToString().ToLower()}";

                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                // ارسال درخواست به ورکر کلودفلر
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                // دریافت پاسخ متنی به صورت استریم زنده شبکه (فوق‌العاده بهینه و سریع)
                using Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                // پارس ناهمگام استریم جی‌سان مینی‌مال
                using JsonDocument doc = await JsonDocument.ParseAsync(responseStream).ConfigureAwait(false);
                JsonElement root = doc.RootElement;

                // استخراج مستقیم فیلدها از ریشه
                string title = root.TryGetProperty("Title", out var titleElement)
                    ? titleElement.GetString() ?? "بدون عنوان"
                    : "بدون عنوان";

                decimal price = root.TryGetProperty("Price", out var priceElement)
                    ? priceElement.GetDecimal()
                    : 0;

                string? imageUrl = root.TryGetProperty("ImageUrl", out var imgElement)
                    ? imgElement.GetString()
                    : null;

                // ۲. استخراج رشته Base64 تصویر و تبدیل آن به آرایه بایت (byte[])
                byte[]? imageBytes = null;
                if (root.TryGetProperty("ImageBytes", out var bytesElement) && bytesElement.ValueKind != JsonValueKind.Null)
                {
                    string? base64String = bytesElement.GetString();
                    if (!string.IsNullOrEmpty(base64String))
                    {
                        try
                        {
                            imageBytes = Convert.FromBase64String(base64String);
                        }
                        catch (FormatException ex)
                        {
                            _logger.LogWarning(ex, "Failed to convert ImageBytes Base64 string for DKP: {DkpId}", digiKalaId);
                        }
                    }
                }

                // اگر عنوان "بدون عنوان" باشد یا ساختار جی‌سان نامعتبر باشد، کالا یافت نشده است
                if (title == "بدون عنوان" && price == 0 && string.IsNullOrEmpty(imageUrl))
                {
                    return Result.Failure<DigiKalaProductResultDto>(BotMessages.ProductNotFoundError);
                }

                // لاگ زدن وضعیت محصولات ناموجود یا بدون تنوع فعال
                if (price == 0)
                {
                    _logger.LogInformation("Product DKP: {DkpId} has 0 price or no default variant. It might be out of stock.", digiKalaId);
                }

                var dto = new DigiKalaProductResultDto(title, price, imageUrl, imageBytes);
                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing optimized product details from DigiKala Proxy for DKP: {DkpId}", digiKalaId);
            }

            return Result.Failure<DigiKalaProductResultDto>(BotMessages.ProductNotFoundError);
        }
        #endregion

        public Result<string> ExtractProductIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure<string>(BotMessages.InvalidLinkError);

            var match = Regex.Match(url, @"dkp-(\d+)", RegexOptions.IgnoreCase);
            if (match.Success) return match.Groups[1].Value;

            if (long.TryParse(url.Trim(), out _)) return url.Trim();

            return Result.Failure<string>(BotMessages.InvalidLinkError);
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
