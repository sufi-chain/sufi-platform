using SufiChain.SufiAbp.AuditLogging;
﻿using System;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.AuditLogging;

[DisableAuditing]
public class AuditLogAction : Entity<Guid>, IMultiTenant, IHasExtraProperties
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid AuditLogId { get; protected set; }

    public virtual string ServiceName { get; protected set; }

    public virtual string MethodName { get; protected set; }

    public virtual string Parameters { get; protected set; }

    public virtual DateTime ExecutionTime { get; protected set; }

    public virtual int ExecutionDuration { get; protected set; }

    public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }

    protected AuditLogAction()
    {
    }

    public AuditLogAction(Guid id, Guid auditLogId, AuditLogActionInfo actionInfo, Guid? tenantId = null)
    {

        Id = id;
        TenantId = tenantId;
        AuditLogId = auditLogId;
        ExecutionTime = actionInfo.ExecutionTime;
        ExecutionDuration = actionInfo.ExecutionDuration;
        ExtraProperties = new ExtraPropertyDictionary(actionInfo.ExtraProperties);
        ServiceName = (actionInfo.ServiceName ?? string.Empty).TruncateFromBeginning(AuditLogActionConsts.MaxServiceNameLength);
        MethodName = (actionInfo.MethodName ?? string.Empty).TruncateFromBeginning(AuditLogActionConsts.MaxMethodNameLength);
        Parameters = actionInfo.Parameters?.Length > AuditLogActionConsts.MaxParametersLength ? string.Empty : actionInfo.Parameters ?? string.Empty;
    }
}
