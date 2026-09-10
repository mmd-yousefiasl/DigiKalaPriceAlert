using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.Dtos.DkMarketplace.Common
{
    public abstract class DkApiServiceBase
    {
        #region Fields
        protected readonly HttpClient HttpClient;
        protected readonly ILogger Logger;

        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
        #endregion

        #region Ctor
        protected DkApiServiceBase(HttpClient httpClient, ILogger logger)
        {
            HttpClient = httpClient;
            Logger = logger;
        }
        #endregion

        #region HTTP Helpers
        protected async Task<Result<T>> GetAsync<T>(string endpoint, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await HttpClient.GetAsync(endpoint, ct);
                return await HandleAsync<T>(endpoint, response, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "DK request timed out. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Request to DK service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "DK HTTP error. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>($"HTTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error calling DK. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Unexpected error occurred.");
            }
        }

        protected async Task<Result<T>> PostAsync<T>(string endpoint, object? body, CancellationToken ct) where T : class
        {
            try
            {
                using var response = body is null
                    ? await HttpClient.PostAsync(endpoint, null, ct)
                    : await HttpClient.PostAsJsonAsync(endpoint, body, JsonOptions, ct);

                return await HandleAsync<T>(endpoint, response, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "DK request timed out. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Request to DK service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "DK HTTP error. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>($"HTTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error calling DK. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Unexpected error occurred.");
            }
        }

        protected async Task<Result<T>> PutAsync<T>(string endpoint, object? body, CancellationToken ct) where T : class
        {
            try
            {
                using var response = body is null
                    ? await HttpClient.PutAsync(endpoint, null, ct)
                    : await HttpClient.PutAsJsonAsync(endpoint, body, JsonOptions, ct);

                return await HandleAsync<T>(endpoint, response, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "DK request timed out. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Request to DK service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "DK HTTP error. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>($"HTTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error calling DK. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Unexpected error occurred.");
            }
        }

        protected async Task<Result<T>> PatchAsync<T>(string endpoint, object? body, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await HttpClient.PatchAsJsonAsync(endpoint, body, JsonOptions, ct);
                return await HandleAsync<T>(endpoint, response, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "DK request timed out. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Request to DK service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "DK HTTP error. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>($"HTTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error calling DK. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Unexpected error occurred.");
            }
        }

        protected async Task<Result<T>> DeleteAsync<T>(string endpoint, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await HttpClient.DeleteAsync(endpoint, ct);
                return await HandleAsync<T>(endpoint, response, ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                Logger.LogError(ex, "DK request timed out. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Request to DK service timed out.");
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "DK HTTP error. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>($"HTTP error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error calling DK. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Unexpected error occurred.");
            }
        }
        #endregion

        #region Internal
        private async Task<Result<T>> HandleAsync<T>(string endpoint, HttpResponseMessage response, CancellationToken ct) where T : class
        {
            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(ct);
                Logger.LogWarning(
                    "DK call failed. Endpoint: {Endpoint}, Status: {Status}, Body: {Body}",
                    endpoint, (int)response.StatusCode, raw);

                return Result.Failure<T>(MapError((int)response.StatusCode, raw));
            }

            var envelope = await response.Content.ReadFromJsonAsync<DkEnvelope<T>>(JsonOptions, ct);

            if (envelope?.Data is null)
            {
                Logger.LogWarning("DK response had no data. Endpoint: {Endpoint}", endpoint);
                return Result.Failure<T>("Empty response from DK service.");
            }

            return Result.Success(envelope.Data);
        }

        private static string MapError(int statusCode, string rawBody)
        {
            if (statusCode == 400)
            {
                try
                {
                    var err = JsonSerializer.Deserialize<DkErrorEnvelope>(rawBody, JsonOptions);
                    if (err?.Errors is { Count: > 0 })
                    {
                        var flat = string.Join("; ",
                            err.Errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));
                        return err.Message is null ? flat : $"{err.Message} — {flat}";
                    }
                    if (!string.IsNullOrWhiteSpace(err?.Message))
                        return err!.Message!;
                }
                catch { }
            }

            return statusCode switch
            {
                400 => "Validation error! check your inputs.",
                401 => "Your access is denied. Please get a new access_token.",
                403 => "Access denied. You may need a new permission.",
                404 => "Data does not exist for your request.",
                429 => "Rate limit reached. Try again after reset_time.",
                500 => "Server issue. Contact support team.",
                _ => $"Request failed with status {statusCode}. {rawBody}"
            };
        }
        #endregion
    }
}
