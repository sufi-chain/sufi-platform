using System;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>Why a tool call was refused at dispatch (sufi-chain#384).</summary>
public static class AgentCallErrorCodes
{
    public const string CallContextMissing = "SufiAI:AgentCallContextMissing";
    public const string PrincipalMismatch = "SufiAI:AgentCallPrincipalMismatch";
    public const string TenantMismatch = "SufiAI:AgentCallTenantMismatch";
    public const string ForgedContext = "SufiAI:AgentCallForgedContext";
    public const string PermissionDenied = "SufiAI:AgentToolPermissionDenied";
    public const string AnonymousNotAllowed = "SufiAI:AgentToolAnonymousNotAllowed";
    public const string PermissionNotDeclared = "SufiAI:AgentToolPermissionNotDeclared";
    public const string IdentityServicesMissing = "SufiAI:AgentCallIdentityServicesMissing";
}

/// <summary>
/// Pure checks run before any tool code (sufi-chain#384 slice 1). A call is refused unless it carries an
/// <see cref="AgentCallContext"/> whose principal, tenant and the workspace ids all agree.
/// </summary>
public static class AgentCallBinding
{
    /// <summary>Returns null when the call may proceed, otherwise an <see cref="AgentCallErrorCodes"/> code.</summary>
    public static string? Validate(WorkspaceContext? workspace)
    {
        var call = workspace?.CallContext;
        if (call is null)
        {
            return AgentCallErrorCodes.CallContextMissing;
        }

        var principalUserId = AgentCallContext.ReadGuidClaim(call.Principal, AbpClaimTypes.UserId);
        if (principalUserId != call.UserId)
        {
            return AgentCallErrorCodes.PrincipalMismatch;
        }

        var principalTenantId = AgentCallContext.ReadGuidClaim(call.Principal, AbpClaimTypes.TenantId);
        if (principalTenantId is not null && principalTenantId != call.TenantId)
        {
            return AgentCallErrorCodes.TenantMismatch;
        }

        // Workspace ids are informational, but a value that disagrees with the bound identity is a forgery.
        if (workspace!.TenantId is not null && workspace.TenantId != call.TenantId)
        {
            return AgentCallErrorCodes.ForgedContext;
        }

        if (workspace.UserId is not null && workspace.UserId != call.UserId)
        {
            return AgentCallErrorCodes.ForgedContext;
        }

        if (workspace.Metadata is { } metadata
            && (Disagrees(metadata, "TenantId", call.TenantId) || Disagrees(metadata, "UserId", call.UserId)))
        {
            return AgentCallErrorCodes.ForgedContext;
        }

        return null;
    }

    private static bool Disagrees(System.Collections.Generic.IDictionary<string, object?> metadata, string key, Guid? bound)
    {
        if (!metadata.TryGetValue(key, out var raw) || raw is null)
        {
            return false;
        }

        Guid? value = raw switch
        {
            Guid guid => guid,
            string text when Guid.TryParse(text, out var parsed) => parsed,
            _ => null
        };

        return value is null || value != bound;
    }
}
