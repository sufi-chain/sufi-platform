using System;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// The identity an audit row is bound to, lifted off <see cref="AgentCallContext"/> once so the write
/// can run in its own scope without depending on the turn's ambient tenant or principal.
/// </summary>
public sealed record AgentCallIdentity(
    Guid? TenantId,
    Guid? UserId,
    string? SessionId,
    Guid? HooshvareId,
    AgentToolScope? AgentScope)
{
    /// <summary>An identity read from the turn's call context, or the workspace fallback ids when it is missing.</summary>
    public static AgentCallIdentity From(WorkspaceContext? context)
    {
        var call = context?.CallContext;
        if (call is not null)
        {
            return new AgentCallIdentity(
                call.TenantId ?? context!.TenantId,
                call.UserId ?? context!.UserId,
                call.SessionId,
                call.HooshvareId,
                call.AgentScope);
        }

        return new AgentCallIdentity(context?.TenantId, context?.UserId, null, null, null);
    }
}
