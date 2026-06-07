using System.Security.Claims;

namespace SufiChain.Chat.Realtime;

/// <summary>
/// Default ticket protector that performs no protection. Active when no hosting integration
/// (such as Blazor Server) registers a real implementation. Registered explicitly via
/// <c>TryAdd</c> so a host-specific implementation always wins regardless of module load order.
/// </summary>
public class NullChatHubTicketProtector : IChatHubTicketProtector
{
    public string? Protect(ClaimsPrincipal principal) => null;

    public ClaimsPrincipal? Unprotect(string ticket) => null;
}
