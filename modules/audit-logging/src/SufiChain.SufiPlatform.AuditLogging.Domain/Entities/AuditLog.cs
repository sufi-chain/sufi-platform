using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.AuditLogging;

[DisableAuditing]
public class AuditLog : AggregateRoot<Guid>, IMultiTenant
{
    public virtual string ApplicationName { get; set; }

    public virtual Guid? UserId { get; protected set; }

    public virtual string UserName { get; protected set; }

    public virtual Guid? TenantId { get; protected set; }

    public virtual string TenantName { get; protected set; }

    public virtual Guid? ImpersonatorUserId { get; protected set; }

    public virtual string ImpersonatorUserName { get; protected set; }

    public virtual Guid? ImpersonatorTenantId { get; protected set; }

    public virtual string ImpersonatorTenantName { get; protected set; }

    public virtual DateTime ExecutionTime { get; protected set; }

    public virtual int ExecutionDuration { get; protected set; }

    public virtual string ClientIpAddress { get; protected set; }

    public virtual string ClientName { get; protected set; }

    public virtual string ClientId { get; set; }

    public virtual string CorrelationId { get; set; }

    public virtual string BrowserInfo { get; protected set; }

    public virtual string HttpMethod { get; protected set; }

    public virtual string Url { get; protected set; }

    public virtual string Exceptions { get; protected set; }

    public virtual string Comments { get; protected set; }

    public virtual int? HttpStatusCode { get; set; }

    public virtual ICollection<EntityChange> EntityChanges { get; protected set; }

    public virtual ICollection<AuditLogAction> Actions { get; protected set; }

    protected AuditLog()
    {

    }

    public AuditLog(
        Guid id,
        string applicationName,
        Guid? tenantId,
        string tenantName,
        Guid? userId,
        string userName,
        DateTime executionTime,
        int executionDuration,
        string clientIpAddress,
        string clientName,
        string clientId,
        string correlationId,
        string browserInfo,
        string httpMethod,
        string url,
        int? httpStatusCode,
        Guid? impersonatorUserId,
        string impersonatorUserName,
        Guid? impersonatorTenantId,
        string impersonatorTenantName,
        ExtraPropertyDictionary extraPropertyDictionary,
        List<EntityChange> entityChanges,
        List<AuditLogAction> actions,
        string exceptions,
        string comments)
        : base(id)
    {
        ApplicationName = (applicationName ?? string.Empty).Truncate(AuditLogConsts.MaxApplicationNameLength);
        TenantId = tenantId;
        TenantName = (tenantName ?? string.Empty).Truncate(AuditLogConsts.MaxTenantNameLength);
        UserId = userId;
        UserName = (userName ?? string.Empty).Truncate(AuditLogConsts.MaxUserNameLength);
        ExecutionTime = executionTime;
        ExecutionDuration = executionDuration;
        ClientIpAddress = (clientIpAddress ?? string.Empty).Truncate(AuditLogConsts.MaxClientIpAddressLength);
        ClientName = (clientName ?? string.Empty).Truncate(AuditLogConsts.MaxClientNameLength);
        ClientId = (clientId ?? string.Empty).Truncate(AuditLogConsts.MaxClientIdLength);
        CorrelationId = (correlationId ?? string.Empty).Truncate(AuditLogConsts.MaxCorrelationIdLength);
        BrowserInfo = (browserInfo ?? string.Empty).Truncate(AuditLogConsts.MaxBrowserInfoLength);
        HttpMethod = (httpMethod ?? string.Empty).Truncate(AuditLogConsts.MaxHttpMethodLength);
        Url = (url ?? string.Empty).Truncate(AuditLogConsts.MaxUrlLength);
        HttpStatusCode = httpStatusCode;
        ImpersonatorUserId = impersonatorUserId;
        ImpersonatorUserName = (impersonatorUserName ?? string.Empty).Truncate(AuditLogConsts.MaxUserNameLength);
        ImpersonatorTenantId = impersonatorTenantId;
        ImpersonatorTenantName = (impersonatorTenantName ?? string.Empty).Truncate(AuditLogConsts.MaxTenantNameLength);
        ExtraProperties = extraPropertyDictionary ?? new ExtraPropertyDictionary();
        EntityChanges = entityChanges ?? new List<EntityChange>();
        Actions = actions ?? new List<AuditLogAction>();
        Exceptions = exceptions ?? string.Empty;
        Comments = (comments ?? string.Empty).Truncate(AuditLogConsts.MaxCommentsLength);
    }
}
