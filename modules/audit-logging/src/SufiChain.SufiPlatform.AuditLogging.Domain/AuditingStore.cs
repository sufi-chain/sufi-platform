using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.AuditLogging;

[Dependency(ReplaceServices = true)]
public class AuditingStore : IAuditingStore, ITransientDependency
{
    public ILogger<AuditingStore> Logger { get; set; }

    protected IAuditLogRepository AuditLogRepository { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected AbpAuditingOptions Options { get; }
    protected IAuditLogInfoToAuditLogConverter Converter { get; }

    public AuditingStore(
        IAuditLogRepository auditLogRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<AbpAuditingOptions> options,
        IAuditLogInfoToAuditLogConverter converter)
    {
        AuditLogRepository = auditLogRepository;
        UnitOfWorkManager = unitOfWorkManager;
        Options = options.Value;
        Converter = converter;
        Logger = NullLogger<AuditingStore>.Instance;
    }

    public virtual async Task SaveAsync(AuditLogInfo auditInfo)
    {
        if (!Options.HideErrors)
        {
            await SaveLogAsync(auditInfo);
            return;
        }

        try
        {
            await SaveLogAsync(auditInfo);
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Could not save the audit log object: {AuditLog}", auditInfo);
            Logger.LogError(ex, "Could not save the audit log object.");
        }
    }

    protected virtual async Task SaveLogAsync(AuditLogInfo auditInfo)
    {
        if (IsBlazorTransportAudit(auditInfo))
        {
            return;
        }

        using var uow = UnitOfWorkManager.Begin(requiresNew: true);
        await AuditLogRepository.InsertAsync(await Converter.ConvertAsync(auditInfo));
        await uow.CompleteAsync();
    }

    protected virtual bool IsBlazorTransportAudit(AuditLogInfo auditInfo)
    {
        return auditInfo.Url != null &&
               (auditInfo.Url.Equals("/_blazor", StringComparison.OrdinalIgnoreCase) ||
                auditInfo.Url.StartsWith("/_blazor/", StringComparison.OrdinalIgnoreCase) ||
                auditInfo.Url.StartsWith("/_blazor?", StringComparison.OrdinalIgnoreCase));
    }
}
