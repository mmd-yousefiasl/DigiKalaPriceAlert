using Application.Dtos.DigiKala;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace Application.Services.Implementaions
{
    public sealed class DigiKalaProductApiClient : IDigiKalaProductApiClient
    {
        #region Fields
        private readonly HttpClient _http;
        private readonly ILogger<DigiKalaProductApiClient> _logger;
        private readonly DigiKalaSessionState _session;
        #endregion

        #region Ctor
        public DigiKalaProductApiClient(
            HttpClient http,
            ILogger<DigiKalaProductApiClient> logger,
            DigiKalaSessionState session)
        {
            _http = http;
            _logger = logger;
            _session = session;

            // Configure headers once
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("fa-IR,fa;q=0.9,en-US;q=0.8,en;q=0.7");
            _http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            _http.DefaultRequestHeaders.Referrer = new Uri("https://www.digikala.com/");
            _http.DefaultRequestHeaders.Add("X-Web-Client", "DesktopWeb");
        }
        #endregion

        public async Task<Result<DigiKalaApiResponse>> GetProductAsync(string digiKalaId, CancellationToken ct)
        {
            await EnsureSessionWarmedUpAsync(ct);

            try
            {
                using var response = await _http.GetAsync(
                    $"https://api.digikala.com/v2/product/{digiKalaId}/",
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.Forbidden)
                {
                    _logger.LogCritical("DigiKala block detected ({Status}) for DKP {Id}", response.StatusCode, digiKalaId);
                    return Result.Failure<DigiKalaApiResponse>(DigiKalaErrors.RateLimited);
                }

                response.EnsureSuccessStatusCode();

                // Deserialize directly to our DTO!
                var apiResponse = await response.Content.ReadFromJsonAsync<DigiKalaApiResponse>(cancellationToken: ct);
                return Result.Success(apiResponse!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP fetch failed for DKP {Id}", digiKalaId);
                return Result.Failure<DigiKalaApiResponse>(DigiKalaErrors.HttpFetchFailed);
            }
        }

        private async Task EnsureSessionWarmedUpAsync(CancellationToken ct)
        {
            if (_session.IsWarmedUp) return;

            await _session.Lock.WaitAsync(ct);
            try
            {
                if (_session.IsWarmedUp) return;

                _logger.LogInformation("Warming up DigiKala session...");
                try
                {
                    using var _ = await _http.GetAsync("https://www.digikala.com/", ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Session warm-up failed; continuing without it.");
                }
                finally
                {
                    _session.MarkWarmedUp();
                }
            }
            finally
            {
                _session.Lock.Release();
            }
        }
    }
}