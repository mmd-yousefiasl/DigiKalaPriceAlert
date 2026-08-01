using Application.Dtos.DigiKala;
using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;

namespace Application.Services.Implementations
{
    public sealed class DigiKalaService : IDigiKalaService
    {
        private readonly IDigiKalaProductApiClient _apiClient;
        private readonly IDigiKalaProductParser _parser;

        public DigiKalaService(IDigiKalaProductApiClient apiClient, IDigiKalaProductParser parser)
        {
            _apiClient = apiClient;
            _parser = parser;
        }

        public Result<string> ExtractProductIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Result.Failure<string>(DigiKalaErrors.InvalidUrlFormat);

            var match = DigiKalaPatterns.ProductIdRegex().Match(url);
            if (match.Success)
                return match.Groups[1].Value;

            var trimmed = url.Trim();
            return long.TryParse(trimmed, out _)
                ? trimmed
                : Result.Failure<string>(DigiKalaErrors.InvalidUrlFormat);
        }

        public async Task<Result<ClientSideScrapedProductDto>> ScrapeFullProductAsync(
            string digiKalaId, CancellationToken ct = default)
        {
            var fetchResult = await _apiClient.GetProductAsync(digiKalaId, ct);
            if (fetchResult.IsFailure)
                return Result.Failure<ClientSideScrapedProductDto>(fetchResult.Message!);

            return _parser.Parse(digiKalaId, fetchResult.Value);
        }
    }
}