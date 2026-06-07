using System.Security.Claims;

namespace SufiChain.Chat.Realtime;

/// <summary>
/// Protects/validates a short-lived ticket that carries the caller identity to the chat hub.
/// Used by hosting models (e.g. Blazor Server) where the outbound hub connection is made
/// from the server process and cannot carry the user's auth cookie. The default implementation
/// is a no-op; hosting integrations replace it with a real (e.g. data-protection based) implementation.
/// </summary>
public interface IChatHubTicketProtector
{
    /// <summary>Creates a protected ticket for the given authenticated principal, or returns null if unsupported.</summary>
    string? Protect(ClaimsPrincipal principal);

    /// <summary>Validates a protected ticket and returns the contained principal, or null if invalid/unsupported.</summary>
    ClaimsPrincipal? Unprotect(string ticket);
}
