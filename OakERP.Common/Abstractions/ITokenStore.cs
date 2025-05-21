namespace OakERP.Common.Abstractions;

public interface ITokenStore
{
    Task SaveTokenAsync(string token);

    Task<string?> GetTokenAsync();

    Task DeleteTokenAsync();
}