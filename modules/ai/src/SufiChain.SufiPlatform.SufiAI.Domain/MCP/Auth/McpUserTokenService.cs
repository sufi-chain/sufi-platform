using System;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Auth;

/// <summary>
/// sufi-chain#384 slice 6b: data-protection based <see cref="IMcpUserTokenService"/>.
/// <para>
/// The payload is serialized and encrypted with the host's data-protection keys, so the same key ring
/// that protects the chat hub ticket protects this token - no new signing scheme. Expiry is carried
/// inside the protected payload (<see cref="McpUserTokenPayload.ExpiresAt"/>) and checked on unprotect,
/// which keeps this project on the plain <see cref="IDataProtector"/> surface.
/// </para>
/// <para>
/// A token is also bound to one server id, so a token leaked from tenant A's server cannot be replayed
/// against tenant B's server, or against another server in the same tenant.
/// </para>
/// </summary>
public class McpUserTokenService : IMcpUserTokenService, ITransientDependency
{
    private readonly IDataProtector _protector;
    private readonly McpUserTokenOptions _options;
    private readonly IClock _clock;

    public McpUserTokenService(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<McpUserTokenOptions> options,
        IClock clock)
    {
        _options = options.Value;
        _clock = clock;
        _protector = dataProtectionProvider.CreateProtector(
            string.IsNullOrWhiteSpace(_options.ProtectorPurpose)
                ? new McpUserTokenOptions().ProtectorPurpose
                : _options.ProtectorPurpose);
    }

    public string? Issue(AgentCallContext call, Guid serverId)
    {
        ArgumentNullException.ThrowIfNull(call);

        if (call.UserId is not { } userId || userId == Guid.Empty)
        {
            // Anonymous turns are never issued a user-scoped token.
            return null;
        }

        var payload = new McpUserTokenPayload
        {
            TenantId = call.TenantId,
            UserId = userId,
            ServerId = serverId,
            SessionId = call.SessionId,
            HooshvareId = call.HooshvareId,
            AgentScope = call.AgentScope is null ? null : (int)call.AgentScope.Value,
            ExpiresAt = UtcNow.Add(Lifetime)
        };

        var json = JsonSerializer.SerializeToUtf8Bytes(payload);
        return _protector.Protect(Convert.ToBase64String(json));
    }

    public McpUserTokenPayload? Validate(string? token, Guid serverId)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            var unprotected = _protector.Unprotect(token);
            var parsed = JsonSerializer.Deserialize<McpUserTokenPayload>(Convert.FromBase64String(unprotected));

            if (parsed is null || parsed.UserId == Guid.Empty)
            {
                return null;
            }

            if (parsed.ServerId != serverId)
            {
                // A token minted for another server (or another tenant's row) is not usable here.
                return null;
            }

            if (parsed.ExpiresAt <= UtcNow)
            {
                return null;
            }

            return parsed;
        }
        catch (Exception)
        {
            // Expired, tampered with, or malformed - treat as no token at all.
            return null;
        }
    }

    private TimeSpan Lifetime =>
        // A non-positive lifetime would mint an already-expired token; keep the documented 5 minute floor.
        _options.LifetimeMinutes <= 0
            ? TimeSpan.FromMinutes(5)
            : TimeSpan.FromMinutes(_options.LifetimeMinutes);

    /// <summary>Now, from the ambient clock so a test can move it and expiry stays deterministic.</summary>
    private DateTimeOffset UtcNow => new(_clock.Now.ToUniversalTime(), TimeSpan.Zero);
}
