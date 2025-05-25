using System.Net.Http.Json;

namespace OakERP.Shared.Services.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest payload
    )
    {
        try
        {
            Console.WriteLine($"HttpClient BaseAddress: {_http.BaseAddress}");
            Console.WriteLine($"URL: {url}");

            var response = await _http.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>();
                return ApiResult<TResponse>.Ok(data!);
            }

            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<TResponse>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Fail(ex.Message);
        }
    }

    public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string url)
    {
        try
        {
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>();
                return ApiResult<TResponse>.Ok(data!);
            }

            var error = await response.Content.ReadAsStringAsync();
            return ApiResult<TResponse>.Fail(error);
        }
        catch (Exception ex)
        {
            return ApiResult<TResponse>.Fail(ex.Message);
        }
    }
}