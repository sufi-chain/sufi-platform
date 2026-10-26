using System;
using System.Linq;
using System.Security.Claims;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// The verified end user a tool or MCP call runs for (sufi-chain#384): user + tenant + session.
/// Created once at the start of a turn and carried on <see cref="WorkspaceContext.CallContext"/>.
/// Every internal tool call runs inside this identity, never under whatever principal is ambient
/// at execution time.
/// </summary>
public sealed class AgentCallContext
{
    /// <summary>Key used when the context travels through <c>Metadata</c> dictionaries (adapters).</summary>
    public const string MetadataKey = "SufiAI.AgentCallContext";

    private AgentCallContext(
        Guid? tenantId,
        Guid? userId,
        string? sessionId,
        ClaimsPrincipal principal,
        AgentToolScope? agentScope = null,
        Guid? hooshvareId = null)
    {
        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        Principal = principal;
        AgentScope = agentScope;
        HooshvareId = hooshvareId;
    }

    /// <summary>Hooshvare the turn came from, when the turn entry point knew it (audit rows carry it).</summary>
    public Guid? HooshvareId { get; }

    /// <summary>
    /// sufi-chain#384 slice 5: the agent's level. Null means the agent acts with the user's own permissions.
    /// A level only narrows the user's permissions (effective = user âˆ© agent).
    /// </summary>
    public AgentToolScope? AgentScope { get; }

    /// <summary>The same bound identity with the agent's level applied.</summary>
    public AgentCallContext WithAgentScope(AgentToolScope? agentScope) =>
        new(TenantId, UserId, SessionId, Principal, agentScope, HooshvareId);

    /// <summary>The same bound identity carrying the Hooshvare the turn came from.</summary>
    public AgentCallContext WithHooshvare(Guid? hooshvareId) =>
        new(TenantId, UserId, SessionId, Principal, AgentScope, hooshvareId);

    /// <summary>Tenant the call is bound to (null for host).</summary>
    public Guid? TenantId { get; }

    /// <summary>Authenticated user the call is bound to. Null means an anonymous turn.</summary>
    public Guid? UserId { get; }

    /// <summary>Conversation/session the call belongs to, when known.</summary>
    public string? SessionId { get; }

    /// <summary>The principal tool code runs under.</summary>
    public ClaimsPrincipal Principal { get; }

    /// <summary>True when no authenticated user is bound. Anonymous calls never borrow another user.</summary>
    public bool IsAnonymous => UserId is null;

    /// <summary>Binds to an authenticated principal. The principal's user and tenant claims must agree with the ids.</summary>
    public static AgentCallContext ForUser(Guid? tenantId, Guid userId, ClaimsPrincipal principal, string? sessionId = null)
    {
        ArgumentNullException.ThrowIfNull(principal);
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("An authenticated call needs a user id.", nameof(userId));
        }

        return new AgentCallContext(tenantId, userId, sessionId, principal);
    }

    /// <summary>An anonymous turn in a tenant: an empty principal, no user.</summary>
    public static AgentCallContext Anonymous(Guid? tenantId = null, string? sessionId = null, Guid? hooshvareId = null)
    {
        return new AgentCallContext(tenantId, null, sessionId, new ClaimsPrincipal(new ClaimsIdentity()), hooshvareId: hooshvareId);
    }

    /// <summary>
    /// Captures the identity at a turn's entry point from the ambient tenant and principal.
    /// A principal without a user id claim yields an anonymous context.
    /// </summary>
    public static AgentCallContext Capture(
        ICurrentTenant currentTenant,
        ICurrentPrincipalAccessor principalAccessor,
        string? sessionId = null,
        Guid? hooshvareId = null)
    {
        ArgumentNullException.ThrowIfNull(currentTenant);
        ArgumentNullException.ThrowIfNull(principalAccessor);

        var principal = principalAccessor.Principal;
        var userId = ReadGuidClaim(principal, AbpClaimTypes.UserId);
        if (principal is null || userId is null)
        {
            return Anonymous(currentTenant.Id, sessionId, hooshvareId);
        }

        return new AgentCallContext(currentTenant.Id, userId, sessionId, principal, agentScope: null, hooshvareId);
    }

    internal static Guid? ReadGuidClaim(ClaimsPrincipal? principal, string claimType)
    {
        var value = principal?.Claims.FirstOrDefault(claim => claim.Type == claimType)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
