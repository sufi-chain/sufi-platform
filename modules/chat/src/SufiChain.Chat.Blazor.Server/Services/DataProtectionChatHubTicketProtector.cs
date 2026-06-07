using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using SufiChain.Chat.Realtime;

namespace SufiChain.Chat.Blazor.Server.Services;

/// <summary>
/// Data-protection based <see cref="IChatHubTicketProtector"/>. Mints a short-lived, encrypted ticket
/// containing the caller principal so server-side (Blazor Server) loopback hub connections can convey
/// the authenticated user without an auth cookie or OAuth token.
/// </summary>
public class DataProtectionChatHubTicketProtector : IChatHubTicketProtector
{
    public const string ProtectorPurpose = "SufiChain.Chat.Realtime.HubTicket.v1";

    protected static readonly TimeSpan TicketLifetime = TimeSpan.FromMinutes(5);

    protected ITimeLimitedDataProtector Protector { get; }

    public DataProtectionChatHubTicketProtector(IDataProtectionProvider dataProtectionProvider)
    {
        Protector = dataProtectionProvider.CreateProtector(ProtectorPurpose).ToTimeLimitedDataProtector();
    }

    public virtual string? Protect(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            principal.WriteTo(writer);
        }

        return Protector.Protect(Convert.ToBase64String(stream.ToArray()), TicketLifetime);
    }

    public virtual ClaimsPrincipal? Unprotect(string ticket)
    {
        try
        {
            var payload = Protector.Unprotect(ticket);
            using var stream = new MemoryStream(Convert.FromBase64String(payload));
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            return new ClaimsPrincipal(reader);
        }
        catch
        {
            // Expired, tampered, or malformed ticket - treat as anonymous.
            return null;
        }
    }
}
