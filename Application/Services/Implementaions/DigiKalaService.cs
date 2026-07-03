using Application.Dtos.DigiKala;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Application.Services.Implementations
{
    public class DigiKalaService : IDigiKalaService
    {
        #region Fields
        private readonly HttpClient _httpClient;
        private readonly ILogger<DigiKalaService> _logger;

        // Modern Chrome on Windows 11 User-Agent
        private const string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
        #endregion

        #region Ctor
        public DigiKalaService(HttpClient httpClient, ILogger<DigiKalaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        #endregion

        private async Task<Result<DigiKalaProductResultDto>> FetchProductDetailsInternalAsync(string digiKalaId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.digikala.com/v2/product/{digiKalaId}/");

                // Using the fixed, modern header
                request.Headers.Add("User-Agent", _userAgent);
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Accept-Language", "fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7");
                request.Headers.Add("X-Web-Client", "DesktopWeb");
                request.Headers.Add("Referer", "https://www.digikala.com/");
                request.Headers.Add("Origin", "https://www.digikala.com");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                var product = json.GetProperty("data").GetProperty("product");

                string title = product.TryGetProperty("title_fa", out var t) ? t.GetString() ?? "بدون عنوان" : "بدون عنوان";

                decimal price = 0;
                if (product.TryGetProperty("default_variant", out var dv) &&
                    dv.TryGetProperty("price", out var p) &&
                    p.TryGetProperty("selling_price", out var sp))
                {
                    price = sp.GetDecimal() / 10;
                }

                string? imageUrl = null;
                if (product.TryGetProperty("images", out var imgs) &&
                    imgs.TryGetProperty("main", out var main) &&
                    main.TryGetProperty("url", out var urls))
                {
                    imageUrl = urls[0].GetString();
                }

                return Result.Success(new DigiKalaProductResultDto(title, price, imageUrl, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping DKP: {DkpId}", digiKalaId);
                return Result.Failure<DigiKalaProductResultDto>(Messages.OperationFailedError);
            }
        }

        public async Task<Result<DigiKalaProductResultDto>> GetProductDetails(string digiKalaId)
            => await FetchProductDetailsInternalAsync(digiKalaId);

        public Result<string> ExtractProductIdFromUrl(string url)
        {
            var match = Regex.Match(url, @"dkp-(\d+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : (long.TryParse(url.Trim(), out _) ? url.Trim() : Result.Failure<string>(Messages.OperationFailedError));
        }

        public async Task<Result<DigiKalaProductResultDto>> GetCurrentPublicIpAsync()
        {
            int retries = 3;
            while (retries > 0)
            {
                try
                {
                    Console.WriteLine(await _httpClient.GetStringAsync("https://api.ipify.org"));
                    return Result.Failure<DigiKalaProductResultDto>(Messages.OperationFailedError);
                }
                catch (HttpRequestException)
                {
                    retries--;
                    await Task.Delay(2000);
                }
            }
            return Result.Failure<DigiKalaProductResultDto>(Messages.OperationFailedError);
        }
    }
}