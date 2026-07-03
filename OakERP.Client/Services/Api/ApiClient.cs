using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OakERP.Client.Services.Errors;
using OakERP.Common.Dtos.Base;

namespace OakERP.Client.Services.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;
    private readonly IClientErrorHandler _errorHandler;

    private static readonly JsonSerializerOptions CaseInsensitiveOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Style",
        "IDE0290:Use primary constructor",
        Justification = "Breaks on Desktop if we use a primary constructor."
    )]
    public ApiClient(HttpClient http, ILogger<ApiClient> logger, IClientErrorHandler errorHandler)
    {
        _http = http;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest payload
    )
    {
        return await SendAsync<TResponse>("POST", url, () => _http.PostAsJsonAsync(url, payload));
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string url)
    {
        return await SendAsync<TResponse>("GET", url, () => _http.GetAsync(url));
    }

    private async Task<ApiResult<TResponse>> SendAsync<TResponse>(
        string method,
        string url,
        Func<Task<HttpResponseMessage>> sendAsync
    )
    {
        try
        {
            var response = await sendAsync();
            return await HandleResponse<TResponse>(response);
        }
        catch (Exception ex)
        {
            var error = await _errorHandler.HandleAsync(ex, new ClientErrorContext(method, url));

            return ApiResult<TResponse>.Fail(error.Message, error.StatusCode);
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
                    Message = (fallback as BaseResultDto)?.Message ?? "Request failed",
                };
            }
        }
        catch (JsonException ex)
        {
            _logger.LogDebug(ex, "Failed to deserialize TResponse fallback");
            var error = await _errorHandler.HandleAsync(
                ex,
                new ClientErrorContext("DeserializeErrorResponse")
            );

            return ApiResult<TResponse>.Fail(error.Message, statusCode);
        }

        return ApiResult<TResponse>.Fail("Unexpected API error", statusCode);
    }
}
