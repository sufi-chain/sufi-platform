using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Auth;

/// <summary>
/// sufi-chain#384 slice 6b: mints and verifies the short-lived, data-protected token that carries a
/// signed-in user's identity to a <c>UserScoped</c> external MCP server.
/// <para>
/// The token is issued per call and dies within minutes. It carries identifiers only - tenant, user,
/// session, Hooshvare, agent scope, server id and expiry - so a server we own can bind the call to the
/// right user without us handing over any credential or permission list.
/// </para>
/// </summary>
public interface IMcpUserTokenService
{
    /// <summary>
    /// Issues a token for the given identity and server. Returns null when the identity has no user
    /// (an anonymous turn may never act on a user-scoped server).
    /// </summary>
    string? Issue(AgentCallContext call, Guid serverId);

    /// <summary>
    /// Verifies a token and returns its payload, or null when it is expired, tampered with, malformed,
    /// or was minted for a different server.
    /// </summary>
    McpUserTokenPayload? Validate(string? token, Guid serverId);
}
