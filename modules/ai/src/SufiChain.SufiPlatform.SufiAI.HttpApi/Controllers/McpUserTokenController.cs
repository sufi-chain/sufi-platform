using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Auth;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

/// <summary>
/// sufi-chain#384 slice 6b: the verify helper for the per-user external MCP token.
/// <para>
/// An MCP server we own (or any server that opted into <c>UserScoped</c>) calls this with the bearer token
/// it received, and gets back the identity the call is allowed to act as. The endpoint is anonymous by
/// design: the token itself is the credential, it is bound to one server id, and it dies within minutes.
/// </para>
/// <para>
/// This is a verification helper, not an authentication endpoint. It never issues a token, never accepts
/// another server's token, and returns nothing but "invalid" for a bad one.
/// </para>
/// </summary>
[Area(AIRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AIRemoteServiceConsts.RemoteServiceName)]
[Route("api/ai/mcp/user-token")]
[AllowAnonymous]
public class McpUserTokenController : AIController
{
    private readonly IMcpUserTokenService _userTokenService;

    public McpUserTokenController(IMcpUserTokenService userTokenService)
    {
        _userTokenService = userTokenService;
    }

    /// <summary>
    /// Verifies the presented token. The server id is required so a token minted for another server is
    /// refused rather than accepted for whatever server happened to ask.
    /// </summary>
    [HttpPost("verify")]
    public virtual Task<VerifyMcpUserTokenResult> VerifyAsync(VerifyMcpUserTokenInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var token = string.IsNullOrWhiteSpace(input.Token)
            ? ReadBearerToken()
            : input.Token;

        var payload = _userTokenService.Validate(token, input.ServerId);

        if (payload is null)
        {
            return Task.FromResult(new VerifyMcpUserTokenResult
            {
                IsValid = false,
                Reason = "The token is expired, malformed, tampered with, or was minted for another server."
            });
        }

        return Task.FromResult(new VerifyMcpUserTokenResult
        {
            IsValid = true,
            TenantId = payload.TenantId,
            UserId = payload.UserId,
            SessionId = payload.SessionId,
            HooshvareId = payload.HooshvareId,
            AgentScope = payload.AgentScope,
            ExpiresAt = payload.ExpiresAt
        });
    }

    private string? ReadBearerToken()
    {
        var header = Request.Headers.Authorization.FirstOrDefault();
        const string prefix = "Bearer ";
        return header is not null && header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? header[prefix.Length..].Trim()
            : null;
    }
}
