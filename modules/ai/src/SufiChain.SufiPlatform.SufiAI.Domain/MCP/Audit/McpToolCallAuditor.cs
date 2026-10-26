using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: writes one append-only audit row per tool/MCP call.
/// <para>
/// The row is committed in its own non-transactional unit of work under the identity's tenant, so a
/// rolled-back or cancelled turn still leaves the trail, and a failed audit never fails the turn.
/// </para>
/// <para>
/// Every scoped dependency is resolved from a fresh DI scope inside the call, so the auditor itself is
/// safe to resolve from the root provider - the tool registry and the reflection tool factory are
/// singletons and resolve it that way.
/// </para>
/// </summary>
public class McpToolCallAuditor : IMcpToolCallAuditor, ITransientDependency
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<McpToolCallAuditor> _logger;

    public McpToolCallAuditor(
        IServiceScopeFactory scopeFactory,
        IGuidGenerator guidGenerator,
        ILogger<McpToolCallAuditor> logger)
    {
        _scopeFactory = scopeFactory;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task RecordAsync(McpToolCallAuditRecord record, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var services = scope.ServiceProvider;

            // Bind the write to the identity's tenant, not the ambient one (a host turn can run for a tenant).
            var currentTenant = services.GetService<ICurrentTenant>();
            using var tenantChange = currentTenant?.Change(record.Identity.TenantId);

            var correlationId = services.GetService<IMcpAuditCorrelationProvider>()?.GetCorrelationId();

            var row = new McpToolCallAuditLog(_guidGenerator.Create())
                .Bind(record.Identity)
                .ForTool(record.ToolName, record.ToolType, record.IsReadOnly)
                .RecordArguments(McpToolCallAuditArguments.Summarize(record.Parameters))
                .Decide(record.Decision, record.DecisionReason)
                .Complete(record.ResultStatus, record.DurationMs, correlationId);

            var repository = services.GetRequiredService<IMcpToolCallAuditLogRepository>();
            var unitOfWorkManager = services.GetService<IUnitOfWorkManager>();

            if (unitOfWorkManager is null)
            {
                await repository.InsertAsync(row, autoSave: true, cancellationToken: CancellationToken.None);
                return;
            }

            // Caller cancellation must not drop the row: the turn may already be cancelled.
            using var unitOfWork = unitOfWorkManager.Begin(
                new AbpUnitOfWorkOptions { IsTransactional = false },
                requiresNew: true);
            await repository.InsertAsync(row, autoSave: true, cancellationToken: CancellationToken.None);
            await unitOfWork.CompleteAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            // Auditing must never break the turn; log and carry on.
            _logger.LogError(
                exception,
                "Failed to write the MCP tool call audit row. Tool={ToolName}, Decision={Decision}",
                record.ToolName,
                record.Decision);
        }
    }
}
