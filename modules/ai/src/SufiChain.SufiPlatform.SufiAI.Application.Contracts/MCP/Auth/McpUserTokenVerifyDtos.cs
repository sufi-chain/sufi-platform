using System;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Auth;

/// <summary>
/// sufi-chain#384 slice 6b: request body for the verify helper. An MCP server we own (or any server that
/// opted into <c>UserScoped</c>) posts the bearer token it received to
/// <c>POST /api/ai/mcp/user-token/verify</c> and gets the identity the call is allowed to act for.
/// <para>
/// The token is normally presented as <c>Authorization: Bearer</c>. The body is offered as well so a
/// server that cannot send a header (or an administrator debugging by hand) can verify it.
/// </para>
/// </summary>
public class VerifyMcpUserTokenInput
{
    /// <summary>Server id the token must have been minted for. Required: a token is only valid for its own server.</summary>
    public Guid ServerId { get; set; }

    /// <summary>
    /// Token to verify. When empty the <c>Authorization: Bearer</c> header is used instead.
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// sufi-chain#384 slice 6b: result of a verify call. <see cref="IsValid"/> false means the caller must
/// refuse the request; every identity field is null in that case so nothing leaks from a bad token.
/// </summary>
public class VerifyMcpUserTokenResult
{
    public bool IsValid { get; set; }

    /// <summary>Reason when invalid, for the server's own log. Never contains token material.</summary>
    public string? Reason { get; set; }

    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string? SessionId { get; set; }

    public Guid? HooshvareId { get; set; }

    public int? AgentScope { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
