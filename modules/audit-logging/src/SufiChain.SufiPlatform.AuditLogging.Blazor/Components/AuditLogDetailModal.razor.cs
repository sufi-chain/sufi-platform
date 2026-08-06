using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiPlatform.AuditLogging.Dtos;
using SufiChain.SufiPlatform.AuditLogging.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.AuditLogging.Blazor.Components;

public partial class AuditLogDetailModal : AuditLoggingComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadAuditLog = "load-audit-log";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? AuditLogId { get; set; }

    private IAuditLogAppService AuditLogAppService => LazyGetRequiredService(ref _auditLogAppService);
    private IAuditLogAppService? _auditLogAppService;

    private AuditLogDto? _auditLog;
    private int _activeTab = 0;
    private Guid? _loadedAuditLogId;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Load audit log details when modal opens with a new ID
        if (Open && AuditLogId.HasValue && AuditLogId != _loadedAuditLogId)
        {
            await LoadAuditLogAsync();
        }
        else if (!Open)
        {
            // Reset when modal closes
            _loadedAuditLogId = null;
            _auditLog = null;
            _activeTab = 0;
        }
    }

    private Task LoadAuditLogAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (AuditLogId.HasValue)
        {
            _auditLog = await AuditLogAppService.GetAsync(AuditLogId.Value);
            _loadedAuditLogId = AuditLogId;
        }
    }, LoadingKeys.LoadAuditLog);

    private Task Hide()
    {
        return SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private bool HasException => !string.IsNullOrEmpty(_auditLog?.Exceptions);

    private SbColor GetStatusColor(int? statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => SbColor.Success,
            >= 300 and < 400 => SbColor.Info,
            >= 400 and < 500 => SbColor.Warning,
            >= 500 => SbColor.Danger,
            _ => SbColor.Default
        };
    }

    private SbColor GetMethodColor(string? method)
    {
        return method?.ToUpper() switch
        {
            "GET" => SbColor.Info,
            "POST" => SbColor.Success,
            "PUT" => SbColor.Warning,
            "DELETE" => SbColor.Danger,
            "PATCH" => SbColor.Primary,
            _ => SbColor.Default
        };
    }

    private SbColor GetChangeTypeColor(EntityChangeType changeType)
    {
        return changeType switch
        {
            EntityChangeType.Created => SbColor.Success,
            EntityChangeType.Updated => SbColor.Warning,
            EntityChangeType.Deleted => SbColor.Danger,
            _ => SbColor.Default
        };
    }

    private string GetServiceShortName(string? serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) return "";
        var parts = serviceName.Split('.');
        return parts.Length > 0 ? parts[^1] : serviceName;
    }

    private string GetEntityTypeName(string? fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "";
        var parts = fullName.Split('.');
        return parts.Length > 0 ? parts[^1] : fullName;
    }
}
