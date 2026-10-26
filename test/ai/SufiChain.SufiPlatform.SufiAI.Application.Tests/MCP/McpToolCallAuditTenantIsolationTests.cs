using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Audit;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;

/// <summary>
/// sufi-chain#384 slice 6a: the audit trail list is behind <c>SufiAI.McpAudit</c> and is tenant-filtered,
/// so a tenant can never read another tenant's tool calls.
/// </summary>
public class McpToolCallAuditTenantIsolationTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-00000000000a");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-0000-0000-0000-00000000000b");
    private static readonly Guid Alice = Guid.Parse("a11ce000-0000-0000-0000-00000000000a");

    [Fact]
    public async Task Tenant_B_cannot_list_Tenant_A_audit_rows()
    {
        var tenantARowId = Guid.NewGuid();

        using (GetRequiredService<ICurrentTenant>().Change(TenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await GetRequiredService<IMcpToolCallAuditLogRepository>().InsertAsync(
                    Row(tenantARowId, TenantA, "helpdesk.kb.search"),
                    autoSave: true);
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(TenantB))
        {
            var service = GetRequiredService<IMcpToolCallAuditAppService>();
            var result = await service.GetListAsync(new GetMcpToolCallAuditLogsInput());

            result.Items.ShouldNotContain(item => item.Id == tenantARowId);
            result.TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Tenant_A_lists_its_own_row_with_the_audit_detail()
    {
        var rowId = Guid.NewGuid();

        using (GetRequiredService<ICurrentTenant>().Change(TenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await GetRequiredService<IMcpToolCallAuditLogRepository>().InsertAsync(
                    Row(rowId, TenantA, "helpdesk.kb.search"),
                    autoSave: true);
            });

            var service = GetRequiredService<IMcpToolCallAuditAppService>();
            var result = await service.GetListAsync(new GetMcpToolCallAuditLogsInput());

            var item = result.Items.ShouldHaveSingleItem();
            item.Id.ShouldBe(rowId);
            item.UserId.ShouldBe(Alice);
            item.ToolName.ShouldBe("helpdesk.kb.search");
            item.ToolType.ShouldBe(nameof(MCPToolType.Internal));
            item.Decision.ShouldBe(nameof(McpToolCallDecision.Denied));
            item.ResultStatus.ShouldBe(nameof(McpToolCallResultStatus.Failed));
            item.DecisionReason.ShouldBe("SufiAI:AgentToolPermissionDenied");
            item.ArgumentsSummary.ShouldBe("apiKey=redacted, query");
        }
    }

    [Fact]
    public async Task List_filters_by_tool_name_and_decision()
    {
        var matchingId = Guid.NewGuid();

        using (GetRequiredService<ICurrentTenant>().Change(TenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<IMcpToolCallAuditLogRepository>();
                await repository.InsertAsync(Row(matchingId, TenantA, "helpdesk.kb.search"), autoSave: true);
                await repository.InsertAsync(Row(Guid.NewGuid(), TenantA, "crm.contacts.create"), autoSave: true);
            });

            var service = GetRequiredService<IMcpToolCallAuditAppService>();

            var byTool = await service.GetListAsync(new GetMcpToolCallAuditLogsInput { ToolName = "helpdesk.kb.search" });
            byTool.Items.ShouldAllBe(item => item.ToolName == "helpdesk.kb.search");
            byTool.Items.ShouldContain(item => item.Id == matchingId);

            var byDecision = await service.GetListAsync(new GetMcpToolCallAuditLogsInput { Decision = "Allowed" });
            byDecision.TotalCount.ShouldBe(0);
        }
    }

    [Fact]
    public async Task Host_scope_does_not_see_a_tenants_rows()
    {
        using (GetRequiredService<ICurrentTenant>().Change(TenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await GetRequiredService<IMcpToolCallAuditLogRepository>().InsertAsync(
                    Row(Guid.NewGuid(), TenantA, "helpdesk.kb.search"),
                    autoSave: true);
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(null))
        {
            var result = await GetRequiredService<IMcpToolCallAuditAppService>()
                .GetListAsync(new GetMcpToolCallAuditLogsInput());

            result.TotalCount.ShouldBe(0);
        }
    }

    private static McpToolCallAuditLog Row(Guid id, Guid tenantId, string toolName)
    {
        return new McpToolCallAuditLog(id)
            .Bind(new AgentCallIdentity(tenantId, Alice, "session-isolation", null, null))
            .ForTool(toolName, MCPToolType.Internal, isReadOnly: true)
            .RecordArguments("apiKey=redacted, query")
            .Decide(McpToolCallDecision.Denied, "SufiAI:AgentToolPermissionDenied")
            .Complete(McpToolCallResultStatus.Failed, 7, correlationId: null);
    }
}
