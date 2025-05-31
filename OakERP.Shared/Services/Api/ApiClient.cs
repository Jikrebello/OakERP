using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OakERP.Common.DTOs.Base;

namespace OakERP.Shared.Services.Api;

/// <summary>
/// Provides methods for interacting with an external API using HTTP requests.
/// </summary>
/// <remarks>This class is designed to simplify communication with an external API by providing methods for
/// sending HTTP GET and POST requests. It handles serialization and deserialization of request and response payloads,
/// as well as error handling and logging.</remarks>
public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    private static readonly JsonSerializerOptions CaseInsensitiveOptions =
        new() { PropertyNameCaseInsensitive = true };

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0290:Use primary constructor",
        Justification = "Breaks on Desktop if we use a primary constructor."
    )]
    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest payload
    )
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, payload);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during POST to {Url}", url);
            return ApiResult<TResponse>.Fail(ex.Message, (int)HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during GET from {Url}", url);
            return ApiResult<TResponse>.Fail(ex.Message, (int)HttpStatusCode.InternalServerError);
        }
    }

    private async Task<ApiResult<TResponse>> HandleResponse<TResponse>(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        string raw = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var data = JsonSerializer.Deserialize<TResponse>(raw, CaseInsensitiveOptions);
            return ApiResult<TResponse>.Ok(data!, statusCode);
        }

        _logger.LogWarning("API returned {StatusCode}. Raw: {Raw}", statusCode, raw);

        // Deserialize your own custom DTO even on failure
        try
        {
            var fallback = JsonSerializer.Deserialize<TResponse>(raw, CaseInsensitiveOptions);
            if (fallback is not null)
            {
                return new ApiResult<TResponse>
                {
                    Data = fallback,
                    Success = false,
                    StatusCode = statusCode,
                    Message = (fallback as BaseResultDTO)?.Message ?? "Request failed",
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Failed to deserialize TResponse fallback");
        }

        return ApiResult<TResponse>.Fail("Unexpected API error", statusCode);
    }
}
