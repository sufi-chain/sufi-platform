using System;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Auth;

/// <summary>
/// sufi-chain#384 slice 6b: the claims a per-user external MCP token carries. The server we own (or any
/// server that opted into per-user binding) verifies this payload to decide whom the call is acting as.
/// <para>Only identifiers travel; no permissions and no secrets, because the token is short-lived and is
/// verified together with a live permission check on our side.</para>
/// </summary>
public sealed class McpUserTokenPayload
{
    /// <summary>Tenant the call is bound to (null for host).</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Signed-in user the call acts as. A user-scoped token always has one.</summary>
    public Guid UserId { get; set; }

    /// <summary>Server the token was minted for; a token is rejected for any other server.</summary>
    public Guid ServerId { get; set; }

    /// <summary>Conversation/session the call belongs to, when the turn knew it.</summary>
    public string? SessionId { get; set; }

    /// <summary>Hooshvare the turn came from, when the turn knew it.</summary>
    public Guid? HooshvareId { get; set; }

    /// <summary>Agent level in force for the call (null = the user's own permissions).</summary>
    public int? AgentScope { get; set; }

    /// <summary>UTC expiry. The token is time-limited and this is the authoritative value.</summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
