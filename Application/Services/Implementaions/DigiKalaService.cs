using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net;
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
        private readonly DigiKalaSessionState _session;

        private const string _userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36";
        #endregion

        #region Ctor
        public DigiKalaService(
            HttpClient httpClient,
            ILogger<DigiKalaService> logger,
            DigiKalaSessionState session)
        {
            _httpClient = httpClient;
            _logger = logger;
            _session = session;
        }
        #endregion

        public async Task<Result<DigiKalaProductResultDto>> GetProductDetails(string digiKalaId)
            => await FetchProductDetailsInternalAsync(digiKalaId);

        public async Task<Result<ClientSideScrapedProductDto>> ScrapeFullProductAsync(string digiKalaId, CancellationToken ct = default)
            => await ScrapeFullProductInternalAsync(digiKalaId, ct);

        public void ResetSession() => _session.Reset();

        public Result<string> ExtractProductIdFromUrl(string url)
        {
            var match = Regex.Match(url, @"dkp-(\d+)", RegexOptions.IgnoreCase);
            return match.Success
                ? match.Groups[1].Value
                : (long.TryParse(url.Trim(), out _)
                    ? url.Trim()
                    : Result.Failure<string>(Messages.OperationFailedError));
        }

        #region Private Methods

        // ════════════════════════════════════════════════════════════════
        // SESSION WARM-UP (singleton state — runs once per cycle globally)
        // ════════════════════════════════════════════════════════════════

        private async Task EnsureSessionAsync(CancellationToken ct)
        {
            if (_session.IsWarmedUp) return; // fast-path — no lock needed

            await _session.Lock.WaitAsync(ct);
            try
            {
                if (_session.IsWarmedUp) return; // double-check after acquiring lock

                _logger.LogInformation("Warming up DigiKala session...");

                var homeRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.digikala.com/");
                homeRequest.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
                homeRequest.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                homeRequest.Headers.TryAddWithoutValidation("Accept-Language", "fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7");
                homeRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
                homeRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
                homeRequest.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "none");
                homeRequest.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");

                await _httpClient.SendAsync(homeRequest, ct);

                _session.MarkWarmedUp();
                _logger.LogInformation("DigiKala session warmed up successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "DigiKala session warm-up failed, continuing anyway.");
                _session.MarkWarmedUp(); // don't retry on every call
            }
            finally
            {
                _session.Lock.Release();
            }
        }

        // ════════════════════════════════════════════════════════════════
        // HTTP LAYER
        // ════════════════════════════════════════════════════════════════

        private async Task<Result<JsonElement>> FetchRawJsonAsync(string digiKalaId, CancellationToken ct = default)
        {
            await EnsureSessionAsync(ct);

            try
            {
                var request = BuildRequest(digiKalaId);
                var response = await _httpClient.SendAsync(request, ct);

                if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
                {
                    _logger.LogCritical(
                        "DigiKala active block triggered! Status: {StatusCode}. DigiKalaId: {DigiKalaId}",
                        response.StatusCode, digiKalaId);

                    return Result.Failure<JsonElement>($"RATE_LIMITED_{response.StatusCode}");
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                return Result.Success(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP fetch failed for DigiKalaId: {DigiKalaId}", digiKalaId);
                return Result.Failure<JsonElement>(Messages.OperationFailedError);
            }
        }

        private HttpRequestMessage BuildRequest(string digiKalaId)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.digikala.com/v2/product/{digiKalaId}/");

            request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
            request.Headers.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            request.Headers.TryAddWithoutValidation("Accept-Language", "fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            request.Headers.TryAddWithoutValidation("X-Web-Client", "DesktopWeb");
            request.Headers.TryAddWithoutValidation("Referer", "https://www.digikala.com/");
            request.Headers.TryAddWithoutValidation("Origin", "https://www.digikala.com");
            request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Not/A)Brand\";v=\"8\", \"Chromium\";v=\"126\", \"Google Chrome\";v=\"126\"");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
            request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-site");

            return request;
        }

        // ════════════════════════════════════════════════════════════════
        // OLD METHOD (kept for backward compatibility)
        // ════════════════════════════════════════════════════════════════

        private async Task<Result<DigiKalaProductResultDto>> FetchProductDetailsInternalAsync(string digiKalaId)
        {
            var jsonResult = await FetchRawJsonAsync(digiKalaId);
            if (jsonResult.IsFailure)
                return Result.Failure<DigiKalaProductResultDto>(Messages.OperationFailedError);

            try
            {
                var product = jsonResult.Value
                    .GetProperty("data")
                    .GetProperty("product");

                var title = product.TryGetProperty("title_fa", out var t)
                    ? t.GetString() ?? "بدون عنوان"
                    : "بدون عنوان";

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
                _logger.LogError(ex, "Error parsing product details for DKP: {DigiKalaId}", digiKalaId);
                return Result.Failure<DigiKalaProductResultDto>(Messages.OperationFailedError);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // NEW METHOD: Full scrape with all variants and per-color buybox
        // ════════════════════════════════════════════════════════════════

        private async Task<Result<ClientSideScrapedProductDto>> ScrapeFullProductInternalAsync(
            string digiKalaId,
            CancellationToken ct)
        {
            var jsonResult = await FetchRawJsonAsync(digiKalaId, ct);
            if (jsonResult.IsFailure)
                return Result.Failure<ClientSideScrapedProductDto>(Messages.OperationFailedError);

            try
            {
                var product = jsonResult.Value
                    .GetProperty("data")
                    .GetProperty("product");

                var variants = ParseVariants(product);

                if (variants.Count == 0)
                {
                    _logger.LogWarning("No parseable variants found for DigiKalaId: {DigiKalaId}", digiKalaId);
                    return Result.Failure<ClientSideScrapedProductDto>("NO_VARIANTS");
                }

                SetBuyBoxWinnersPerColor(variants);

                var scraped = new ClientSideScrapedProductDto
                {
                    DigiKalaId = digiKalaId,
                    Title = ParseTitle(product),
                    Url = ParseUrl(product, digiKalaId),
                    ImageUrl = ParseImageUrl(product),
                    DefaultVariantId = ParseDefaultVariantId(product),
                    Variants = variants
                };

                return Result.Success(scraped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing full product for DigiKalaId: {DigiKalaId}", digiKalaId);
                return Result.Failure<ClientSideScrapedProductDto>(Messages.OperationFailedError);
            }
        }

        // ════════════════════════════════════════════════════════════════
        // PARSING HELPERS
        // ════════════════════════════════════════════════════════════════

        private static string ParseTitle(JsonElement product)
            => product.TryGetProperty("title_fa", out var t)
                ? t.GetString() ?? "بدون عنوان"
                : "بدون عنوان";

        private static string ParseUrl(JsonElement product, string digiKalaId)
            => $"https://www.digikala.com/product/dkp-{digiKalaId}/";

        private static string? ParseImageUrl(JsonElement product)
        {
            if (product.TryGetProperty("images", out var images) &&
                images.TryGetProperty("main", out var main) &&
                main.TryGetProperty("url", out var urls) &&
                urls.GetArrayLength() > 0)
            {
                return urls[0].GetString();
            }
            return null;
        }

        private static long ParseDefaultVariantId(JsonElement product)
            => product.TryGetProperty("default_variant", out var dv) &&
               dv.TryGetProperty("id", out var id)
                ? id.GetInt64()
                : 0;

        private List<ClientSideScrapedVariantDto> ParseVariants(JsonElement product)
        {
            var result = new List<ClientSideScrapedVariantDto>();

            if (!product.TryGetProperty("variants", out var variants))
                return result;

            foreach (var variant in variants.EnumerateArray())
            {
                var parsed = TryParseVariant(variant);
                if (parsed is not null)
                    result.Add(parsed);
            }

            return result;
        }

        private static void SetBuyBoxWinnersPerColor(List<ClientSideScrapedVariantDto> variants)
        {
            var colorGroups = variants
                .Where(v => v.IsMarketable)
                .GroupBy(v => v.ColorName);

            foreach (var group in colorGroups)
            {
                var winner = group
                    .OrderByDescending(v => v.Rank)
                    .First();

                winner.IsBuyBoxWinner = true;
            }
        }

        private ClientSideScrapedVariantDto? TryParseVariant(JsonElement variant)
        {
            try
            {
                if (!variant.TryGetProperty("id", out var idProp))
                    return null;

                if (!variant.TryGetProperty("seller", out var seller))
                    return null;

                if (!seller.TryGetProperty("id", out var sellerId))
                    return null;

                var status = variant.TryGetProperty("status", out var statusProp)
                    ? statusProp.GetString() ?? "unknown"
                    : "unknown";

                decimal rank = 0;
                if (variant.TryGetProperty("rank", out var rankProp))
                    rank = rankProp.GetDecimal();

                decimal price = 0;
                if (variant.TryGetProperty("price", out var priceObj) &&
                    priceObj.TryGetProperty("selling_price", out var sp))
                {
                    price = sp.GetDecimal() / 10;
                }

                if (price == 0 && status != "marketable")
                    return null;

                int? colorId = null;
                string colorName = "بدون رنگ";
                string? colorHex = null;

                if (variant.TryGetProperty("color", out var color))
                {
                    colorId = color.TryGetProperty("id", out var cId) ? cId.GetInt32() : null;
                    colorName = color.TryGetProperty("title", out var cTitle) ? cTitle.GetString() ?? "بدون رنگ" : "بدون رنگ";
                    colorHex = color.TryGetProperty("hex_code", out var cHex) ? cHex.GetString() : null;
                }

                decimal? sellerRating = null;
                if (seller.TryGetProperty("rating", out var rating) &&
                    rating.TryGetProperty("total_rate", out var rateVal))
                {
                    sellerRating = rateVal.GetDecimal();
                }

                return new ClientSideScrapedVariantDto
                {
                    DigiKalaVariantId = idProp.GetInt64(),
                    Status = status,
                    Rank = rank,
                    IsBuyBoxWinner = false,

                    ColorId = colorId,
                    ColorName = colorName,
                    ColorHex = colorHex,

                    SellerId = sellerId.GetInt64(),
                    SellerName = seller.TryGetProperty("title", out var sTitle) ? sTitle.GetString() ?? "نامشخص" : "نامشخص",
                    SellerCode = seller.TryGetProperty("code", out var sCode) ? sCode.GetString() ?? string.Empty : string.Empty,
                    SellerRating = sellerRating,

                    Price = price
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse a variant entry, skipping it");
                return null;
            }
        }

        #endregion
    }
}