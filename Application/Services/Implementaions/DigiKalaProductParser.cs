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

                var sellers = MapSellers(product.Variants).ToList();
                if (sellers.Count == 0)
                {
                    _logger.LogWarning("No valid sellers for DKP {Id}", digiKalaId);
                    return Result.Failure<ClientSideScrapedProductDto>(DigiKalaErrors.NoVariants);
                }

                // Assign buy box winners based on color groups
                _buyBox.AssignWinners(sellers);

                return Result.Success(new ClientSideScrapedProductDto
                {
                    DigiKalaId = digiKalaId,
                    Title = product.Title ?? DigiKalaDefaults.NoTitle,
                    Url = $"https://www.digikala.com/product/dkp-{digiKalaId}/",
                    ImageUrl = product.Images?.Main?.Urls?.FirstOrDefault(),
                    DefaultVariantId = product.DefaultVariant?.Id ?? 0,
                    Sellers = sellers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Parse failed for DKP {Id}", digiKalaId);
                return Result.Failure<ClientSideScrapedProductDto>(DigiKalaErrors.ParseFailed);
            }
        }

        private IEnumerable<ClientSideScrapedSellerDto> MapSellers(List<DigiKalaVariant>? variants)
        {
            if (variants is null) yield break;

            foreach (var v in variants)
            {
                if (v is null || v.Id == 0 || v.Seller is null || v.Seller.Id == 0)
                    continue;

                var price = (v.Price?.SellingPrice ?? 0) / DigiKalaDefaults.RialPerToman;
                var isMarketable = v.Status == "marketable";

                // Skip unmarketable items with no price
                if (price == 0 && !isMarketable)
                    continue;

                yield return new ClientSideScrapedSellerDto
                {
                    DigiKalaVariantId = v.Id,
                    ColorId = v.Color?.Id,
                    ColorName = v.Color?.Title ?? DigiKalaDefaults.NoColor,
                    ColorHex = v.Color?.HexCode,

                    SellerId = v.Seller.Id,
                    SellerName = v.Seller.Title ?? DigiKalaDefaults.UnknownSeller,
                    SellerCode = v.Seller.Code ?? string.Empty,
                    SellerRating = v.Seller.Rating?.TotalRate,

                    Price = price,
                    Rank = v.Rank,
                    Status = v.Status ?? "unknown",
                    IsBuyBoxWinner = false
                };
            }
        }
    }
}
