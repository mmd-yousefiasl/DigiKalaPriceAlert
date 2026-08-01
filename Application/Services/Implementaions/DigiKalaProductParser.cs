using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public sealed class DigiKalaProductParser : IDigiKalaProductParser
    {
        private readonly ILogger<DigiKalaProductParser> _logger;
        private readonly IBuyBoxStrategy _buyBox;

        public DigiKalaProductParser(ILogger<DigiKalaProductParser> logger, IBuyBoxStrategy buyBox)
        {
            _logger = logger;
            _buyBox = buyBox;
        }

        public Result<ClientSideScrapedProductDto> Parse(string digiKalaId, DigiKalaApiResponse response)
        {
            try
            {
                var product = response.Data?.Product;
                if (product is null)
                    return Result.Failure<ClientSideScrapedProductDto>(DigiKalaErrors.MalformedResponse);

                var variants = MapVariants(product.Variants).ToList();
                if (variants.Count == 0)
                {
                    _logger.LogWarning("No variants for DKP {Id}", digiKalaId);
                    return Result.Failure<ClientSideScrapedProductDto>(DigiKalaErrors.NoVariants);
                }

                _buyBox.AssignWinners(variants);

                return Result.Success(new ClientSideScrapedProductDto
                {
                    DigiKalaId = digiKalaId,
                    Title = product.Title ?? DigiKalaDefaults.NoTitle,
                    Url = $"https://www.digikala.com/product/dkp-{digiKalaId}/",
                    ImageUrl = product.Images?.Main?.Urls?.FirstOrDefault(),
                    DefaultVariantId = product.DefaultVariant?.Id ?? 0,
                    Variants = variants
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse failed for DKP {Id}", digiKalaId);
                return Result.Failure<ClientSideScrapedProductDto>(DigiKalaErrors.ParseFailed);
            }
        }

        private IEnumerable<ClientSideScrapedVariantDto> MapVariants(List<DigiKalaVariant>? variants)
        {
            if (variants is null) yield break;

            foreach (var v in variants)
            {
                if (v is null || v.Id == 0 || v.Seller is null || v.Seller.Id == 0)
                    continue;

                var price = (v.Price?.SellingPrice ?? 0) / DigiKalaDefaults.RialPerToman;

                // Since IsMarketable is computed from Status, we just evaluate it here:
                var isMarketable = v.Status == "marketable";

                // Skip unmarketable items with no price
                if (price == 0 && !isMarketable)
                    continue;

                yield return new ClientSideScrapedVariantDto
                {
                    DigiKalaVariantId = v.Id,
                    Status = v.Status ?? "unknown",
                    // IsMarketable is removed from here! It computes automatically.
                    Rank = v.Rank,
                    IsBuyBoxWinner = false,

                    ColorId = v.Color?.Id,
                    ColorName = v.Color?.Title ?? DigiKalaDefaults.NoColor,
                    ColorHex = v.Color?.HexCode,

                    SellerId = v.Seller.Id,
                    SellerName = v.Seller.Title ?? DigiKalaDefaults.UnknownSeller,
                    SellerCode = v.Seller.Code ?? string.Empty,
                    SellerRating = v.Seller.Rating?.TotalRate,

                    Price = price
                };
            }
        }
    }
}
