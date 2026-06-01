namespace SufiChain.Chat.Blazor.Public.Services;

/// <summary>
/// Provides access tokens for authenticated SignalR hub connections.
/// </summary>
public interface IChatHubConnectionAccessTokenProvider
{
    /// <summary>
    /// Returns the current user's access token, or null when unauthenticated.
    /// </summary>
    Task<string?> GetAccessTokenAsync();
}
