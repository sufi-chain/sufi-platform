namespace SufiChain.Chat.Blazor.Public.Services;

/// <summary>
/// Resolves the absolute chat hub URL for client connections.
/// </summary>
public interface IChatHubUrlResolver
{
    string GetHubUrl();
}
