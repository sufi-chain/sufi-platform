using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: read-only access to the tool/MCP call audit trail.
/// </summary>
public interface IMcpToolCallAuditAppService
{
    Task<PagedResultDto<McpToolCallAuditLogDto>> GetListAsync(GetMcpToolCallAuditLogsInput input);
}
