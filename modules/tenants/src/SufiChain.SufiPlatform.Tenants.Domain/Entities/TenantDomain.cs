using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomain : AuditedEntity<Guid>
{
    public virtual Guid TenantId { get; protected set; }

    public virtual string Host { get; protected set; }

    public virtual TenantDomainType Type { get; protected set; }

    public virtual bool IsVerified { get; protected set; }

    public virtual bool IsActive { get; protected set; }

    protected TenantDomain()
    {
    }

    protected internal TenantDomain(
        Guid id,
        Guid tenantId,
        string host,
        TenantDomainType type,
        bool isVerified,
        bool isActive)
        : base(id)
    {
        TenantId = tenantId;
        Host = TenantDomainName.NormalizeHost(host);
        Type = type;
        SetStatus(isVerified, isActive);
    }

    public virtual void SetStatus(bool isVerified, bool isActive)
    {
        if (Type == TenantDomainType.Custom && isActive && !isVerified)
        {
            throw new BusinessException("TenantManagement:UnverifiedCustomDomainCanNotBeActive")
                .WithData("Host", Host);
        }

        IsVerified = isVerified;
        IsActive = isActive;
    }
}
