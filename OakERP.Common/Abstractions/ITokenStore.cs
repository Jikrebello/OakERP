namespace OakERP.Common.Abstractions;

public interface ITokenStore
{
    Task SaveToken(string token);

    Task<string?> GetToken();

    Task DeleteToken();
}
