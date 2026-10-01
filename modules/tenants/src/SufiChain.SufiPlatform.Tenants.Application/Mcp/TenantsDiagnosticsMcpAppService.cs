using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Tenants.Hooshvare;

namespace SufiChain.SufiPlatform.Tenants.Mcp;

/// <summary>
/// Read-only tenant diagnostics. Does not call connection-string APIs.
/// </summary>
[Authorize]
public class TenantsDiagnosticsMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected ITenantAppService Tenants { get; }

    public TenantsDiagnosticsMcpAppService(ITenantAppService tenants)
    {
        Tenants = tenants;
    }

    [SufiAiMcpTool(TenantsDiagnosticsHooshvareKeys.Tools.List,
        "Lists tenants by name and edition. Omits connection strings and database names.", ReadOnly = true)]
    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual async Task<object> ListAsync(string? filter = null)
    {
        var page = await Tenants.GetListAsync(new GetTenantsInput
        {
            Filter = filter,
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(MapSummary).ToList()
        };
    }

    [SufiAiMcpTool(TenantsDiagnosticsHooshvareKeys.Tools.Get,
        "Returns one tenant without connection strings or database names.", ReadOnly = true)]
    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual async Task<object> GetAsync(Guid id) => MapSummary(await Tenants.GetAsync(id));

    [SufiAiMcpTool(TenantsDiagnosticsHooshvareKeys.Tools.GetSummary,
        "Returns name, active domain status, and edition for one tenant. Never returns connection strings.", ReadOnly = true)]
    [Authorize(TenantsPermissions.Tenants.Default)]
    public virtual async Task<object> GetSummaryAsync(Guid id) => MapSummary(await Tenants.GetAsync(id));

    private static object MapSummary(TenantDto tenant) => new
    {
        tenant.Id,
        tenant.Name,
        tenant.EditionId,
        Active = tenant.Domains.Count == 0 || tenant.Domains.Any(domain => domain.IsActive),
        tenant.PrimarySubdomain,
        Domains = tenant.Domains.Select(domain => new
        {
            domain.Host,
            Type = domain.Type.ToString(),
            domain.IsVerified,
            domain.IsActive
        }).ToList()
    };
}
