using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: writes the audit trail for tool/MCP calls. Implementations must never let a
/// failed audit break the turn - the caller cannot be failed by the audit write.
/// </summary>
public interface IMcpToolCallAuditor
{
    /// <summary>
    /// Records one call (allowed, denied or held for approval). Expected to swallow and log its own
    /// failures so a tool call is never broken by audit persistence.
    /// </summary>
    Task RecordAsync(McpToolCallAuditRecord record, CancellationToken cancellationToken = default);
}
