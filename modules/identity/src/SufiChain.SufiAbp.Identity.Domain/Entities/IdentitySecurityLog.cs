using System;
using JetBrains.Annotations;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SecurityLog;

namespace SufiChain.SufiAbp.Identity;

public class IdentitySecurityLog : Entity<Guid>, IMultiTenant, IHasCreationTime, IHasExtraProperties
{
    public virtual Guid? TenantId { get; protected set; }

    [CanBeNull]
    public virtual string? ApplicationName { get; set; }

    [CanBeNull]
    public virtual string? Identity { get; set; }

    [CanBeNull]
    public virtual string? Action { get; set; }

    public virtual Guid? UserId { get; set; }

    [CanBeNull]
    public virtual string? UserName { get; set; }

    [CanBeNull]
    public virtual string? TenantName { get; set; }

    [CanBeNull]
    public virtual string? ClientId { get; set; }

    [CanBeNull]
    public virtual string? CorrelationId { get; set; }

    [CanBeNull]
    public virtual string? ClientIpAddress { get; set; }

    [CanBeNull]
    public virtual string? BrowserInfo { get; set; }

    public virtual DateTime CreationTime { get; protected set; }

    public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }

    protected IdentitySecurityLog()
    {

    }

    public IdentitySecurityLog(Guid id, Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        CreationTime = DateTime.UtcNow;
        ExtraProperties = new ExtraPropertyDictionary();
    }

    public IdentitySecurityLog(Guid id, SecurityLogInfo securityLogInfo)
        : base(id)
    {
        ApplicationName = securityLogInfo.ApplicationName.Truncate(IdentitySecurityLogConsts.MaxApplicationNameLength);
        Identity = securityLogInfo.Identity.Truncate(IdentitySecurityLogConsts.MaxIdentityLength);
        Action = securityLogInfo.Action.Truncate(IdentitySecurityLogConsts.MaxActionLength);
        UserId = securityLogInfo.UserId;
        UserName = securityLogInfo.UserName.Truncate(IdentitySecurityLogConsts.MaxUserNameLength);
        TenantId = securityLogInfo.TenantId;
        TenantName = securityLogInfo.TenantName.Truncate(IdentitySecurityLogConsts.MaxTenantNameLength);
        ClientId = securityLogInfo.ClientId.Truncate(IdentitySecurityLogConsts.MaxClientIdLength);
        CorrelationId = securityLogInfo.CorrelationId.Truncate(IdentitySecurityLogConsts.MaxCorrelationIdLength);
        ClientIpAddress = securityLogInfo.ClientIpAddress.Truncate(IdentitySecurityLogConsts.MaxClientIpAddressLength);
        BrowserInfo = securityLogInfo.BrowserInfo.Truncate(IdentitySecurityLogConsts.MaxBrowserInfoLength);
        CreationTime = securityLogInfo.CreationTime == default ? DateTime.UtcNow : securityLogInfo.CreationTime;
        ExtraProperties = new ExtraPropertyDictionary(securityLogInfo.ExtraProperties);
    }
}
