using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using SufiChain.SufiPlatform.AuditLogging.Hooshvare;
using SufiChain.SufiPlatform.AuditLogging.Permissions;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.AuditLogging.Mcp;

[Authorize]
public class AuditInvestigatorMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected IAuditLogAppService AuditLogs { get; }
    protected IEntityChangeAppService EntityChanges { get; }

    public AuditInvestigatorMcpAppService(IAuditLogAppService auditLogs, IEntityChangeAppService entityChanges)
    {
        AuditLogs = auditLogs;
        EntityChanges = entityChanges;
    }

    [SufiAiMcpTool(AuditInvestigatorHooshvareKeys.Tools.SearchLogs,
        "Searches audit logs. Results omit action parameter dumps.", ReadOnly = true)]
    [Authorize(AuditLoggingPermissions.AuditLogs.Default)]
    public virtual async Task<object> SearchLogsAsync(Guid? userId = null, string? userName = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var page = await AuditLogs.GetListAsync(new GetAuditLogListInput
        {
            UserId = userId,
            UserName = userName,
            StartTime = fromUtc,
            EndTime = toUtc,
            MaxResultCount = MaxResults,
            IncludeDetails = false
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(item => new
            {
                item.Id,
                item.UserId,
                item.UserName,
                item.ExecutionTime,
                item.ExecutionDuration,
                item.HttpMethod,
                item.Url,
                item.HttpStatusCode,
                item.HasException
            }).ToList()
        };
    }

    [SufiAiMcpTool(AuditInvestigatorHooshvareKeys.Tools.GetLog,
        "Returns one audit log without full action parameter dumps.", ReadOnly = true)]
    [Authorize(AuditLoggingPermissions.AuditLogs.Default)]
    public virtual async Task<object> GetLogAsync(Guid id)
    {
        var log = await AuditLogs.GetAsync(id);
        return new
        {
            log.Id,
            log.UserId,
            log.UserName,
            log.TenantId,
            log.ExecutionTime,
            log.ExecutionDuration,
            log.HttpMethod,
            log.Url,
            log.HttpStatusCode,
            log.Exceptions,
            Actions = log.Actions.Select(action => new
            {
                action.ServiceName,
                action.MethodName,
                action.ExecutionTime,
                action.ExecutionDuration,
                Parameters = "omitted"
            }).ToList()
        };
    }

    [SufiAiMcpTool(AuditInvestigatorHooshvareKeys.Tools.SearchEntityChanges,
        "Searches entity changes. Secret-like property values are redacted.", ReadOnly = true)]
    [Authorize(AuditLoggingPermissions.EntityChanges.Default)]
    public virtual async Task<object> SearchEntityChangesAsync(string? entityType = null, string? entityId = null, Guid? auditLogId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        var page = await EntityChanges.GetListAsync(new GetEntityChangeListInput
        {
            EntityTypeFullName = entityType,
            EntityId = entityId,
            AuditLogId = auditLogId,
            StartTime = fromUtc,
            EndTime = toUtc,
            MaxResultCount = MaxResults,
            IncludeDetails = true
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(MapChange).ToList()
        };
    }

    [SufiAiMcpTool(AuditInvestigatorHooshvareKeys.Tools.GetEntityChange,
        "Returns one entity change with secret-like property values redacted.", ReadOnly = true)]
    [Authorize(AuditLoggingPermissions.EntityChanges.Default)]
    public virtual async Task<object> GetEntityChangeAsync(Guid id)
    {
        return MapChange(await EntityChanges.GetAsync(id));
    }

    private static object MapChange(EntityChangeDto change) => new
    {
        change.Id,
        change.AuditLogId,
        ChangeType = change.ChangeType.ToString(),
        change.EntityTypeFullName,
        change.EntityId,
        change.ChangeTime,
        PropertyChanges = change.PropertyChanges.Select(MapProperty).ToList()
    };

    private static object MapChange(EntityChangeListItemDto change) => new
    {
        change.Id,
        change.AuditLogId,
        change.UserName,
        ChangeType = change.ChangeType.ToString(),
        change.EntityTypeFullName,
        change.EntityId,
        change.ChangeTime,
        PropertyChanges = change.PropertyChanges.Select(MapProperty).ToList()
    };

    private static object MapProperty(EntityPropertyChangeDto change)
    {
        var secret = IsSecretName(change.PropertyName);
        return new
        {
            change.PropertyName,
            change.PropertyTypeFullName,
            OriginalValue = secret ? "[redacted]" : change.OriginalValue,
            NewValue = secret ? "[redacted]" : change.NewValue
        };
    }

    private static bool IsSecretName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var value = name.Replace("_", "", StringComparison.Ordinal).Replace("-", "", StringComparison.Ordinal);
        return value.Contains("password", StringComparison.OrdinalIgnoreCase)
            || value.Contains("token", StringComparison.OrdinalIgnoreCase)
            || value.Contains("secret", StringComparison.OrdinalIgnoreCase)
            || value.Contains("apikey", StringComparison.OrdinalIgnoreCase)
            || value.Contains("connectionstring", StringComparison.OrdinalIgnoreCase)
            || value.Contains("hash", StringComparison.OrdinalIgnoreCase)
            || value.Contains("privateclaim", StringComparison.OrdinalIgnoreCase);
    }
}
