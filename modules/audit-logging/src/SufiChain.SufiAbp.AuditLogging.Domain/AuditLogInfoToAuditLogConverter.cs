using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace SufiChain.SufiAbp.AuditLogging;

public class AuditLogInfoToAuditLogConverter : IAuditLogInfoToAuditLogConverter, ITransientDependency
{
    protected IGuidGenerator GuidGenerator { get; }

    public AuditLogInfoToAuditLogConverter(IGuidGenerator guidGenerator)
    {
        GuidGenerator = guidGenerator;
    }

    public virtual Task<AuditLog> ConvertAsync(AuditLogInfo auditLogInfo)
    {
        var auditLogId = GuidGenerator.Create();
        var extraProperties = new ExtraPropertyDictionary();

        if (auditLogInfo.ExtraProperties != null)
        {
            foreach (var pair in auditLogInfo.ExtraProperties)
            {
                extraProperties[pair.Key] = pair.Value;
            }
        }

        var entityChanges = auditLogInfo.EntityChanges?
            .Select(entityChangeInfo => new EntityChange(GuidGenerator, auditLogId, entityChangeInfo, auditLogInfo.TenantId))
            .ToList() ?? new List<EntityChange>();

        var actions = auditLogInfo.Actions?
            .Select(actionInfo => new AuditLogAction(GuidGenerator.Create(), auditLogId, actionInfo, auditLogInfo.TenantId))
            .ToList() ?? new List<AuditLogAction>();

        var exceptions = auditLogInfo.Exceptions?.Any() == true
            ? auditLogInfo.Exceptions.Select(exception => exception.ToString()).JoinAsString(Environment.NewLine)
            : null;

        var comments = auditLogInfo.Comments?.JoinAsString(Environment.NewLine);

        return Task.FromResult(new AuditLog(
            auditLogId,
            auditLogInfo.ApplicationName,
            auditLogInfo.TenantId,
            auditLogInfo.TenantName,
            auditLogInfo.UserId,
            auditLogInfo.UserName,
            auditLogInfo.ExecutionTime,
            auditLogInfo.ExecutionDuration,
            auditLogInfo.ClientIpAddress,
            auditLogInfo.ClientName,
            auditLogInfo.ClientId,
            auditLogInfo.CorrelationId,
            auditLogInfo.BrowserInfo,
            auditLogInfo.HttpMethod,
            auditLogInfo.Url,
            auditLogInfo.HttpStatusCode,
            auditLogInfo.ImpersonatorUserId,
            auditLogInfo.ImpersonatorUserName,
            auditLogInfo.ImpersonatorTenantId,
            auditLogInfo.ImpersonatorTenantName,
            extraProperties,
            entityChanges,
            actions,
            exceptions,
            comments));
    }
}
