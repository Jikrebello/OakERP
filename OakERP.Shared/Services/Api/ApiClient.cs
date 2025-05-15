using System.Net.Http.Json;

namespace OakERP.Shared.Services.Api;

public class ApiClient(HttpClient http) : IApiClient
{
    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest payload
    )
    {
        try
        {
            var response = await http.PostAsJsonAsync(url, payload);

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
            var response = await http.GetAsync(url);

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
