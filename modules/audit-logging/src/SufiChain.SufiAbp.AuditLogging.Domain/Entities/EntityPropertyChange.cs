using SufiChain.SufiAbp.AuditLogging;
﻿using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.AuditLogging;

[DisableAuditing]
public class EntityPropertyChange : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid EntityChangeId { get; protected set; }

    public virtual string NewValue { get; protected set; }

    public virtual string OriginalValue { get; protected set; }

    public virtual string PropertyName { get; protected set; }

    public virtual string PropertyTypeFullName { get; protected set; }

    protected EntityPropertyChange()
    {

    }

    public EntityPropertyChange(
        IGuidGenerator guidGenerator,
        Guid entityChangeId,
        EntityPropertyChangeInfo entityChangeInfo,
        Guid? tenantId = null)
    {
        Id = guidGenerator.Create();
        TenantId = tenantId;
        EntityChangeId = entityChangeId;
        NewValue = (entityChangeInfo.NewValue ?? string.Empty).Truncate(EntityPropertyChangeConsts.MaxNewValueLength);
        OriginalValue = (entityChangeInfo.OriginalValue ?? string.Empty).Truncate(EntityPropertyChangeConsts.MaxOriginalValueLength);
        PropertyName = (entityChangeInfo.PropertyName ?? string.Empty).TruncateFromBeginning(EntityPropertyChangeConsts.MaxPropertyNameLength);
        PropertyTypeFullName = (entityChangeInfo.PropertyTypeFullName ?? string.Empty).TruncateFromBeginning(EntityPropertyChangeConsts.MaxPropertyTypeFullNameLength);
    }
}
