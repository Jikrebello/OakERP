namespace OakERP.Client.Services.Api;

public interface IApiClient
{
    Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest payload);

    Task<ApiResult<TResponse>> GetAsync<TResponse>(string url);
}
