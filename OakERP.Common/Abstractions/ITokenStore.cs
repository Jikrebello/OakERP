namespace OakERP.Common.Abstractions;

/// <summary>
/// Defines methods for storing, retrieving, and deleting authentication tokens.
/// </summary>
/// <remarks>This interface provides an abstraction for managing tokens, such as access tokens or refresh tokens,
/// in a persistent or in-memory store. Implementations may vary in storage mechanisms, such as file-based,
/// database-backed, or in-memory storage.</remarks>
public interface ITokenStore
{
    /// <summary>
    /// Asynchronously saves the specified token for future use.
    /// </summary>
    /// <param name="token">The token to be saved. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveTokenAsync(string token);

    /// <summary>
    /// Asynchronously retrieves an authentication token.
    /// </summary>
    /// <remarks>The returned token may be used for authenticating requests to external services.  Callers
    /// should handle the possibility of a <see langword="null"/> result if no token is available.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authentication token as a
    /// string,  or <see langword="null"/> if no token is available.</returns>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Deletes the current authentication token asynchronously.
    /// </summary>
    /// <remarks>This method removes the token associated with the current session or user context.  Ensure
    /// that any operations requiring the token are completed before calling this method.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteTokenAsync();
}