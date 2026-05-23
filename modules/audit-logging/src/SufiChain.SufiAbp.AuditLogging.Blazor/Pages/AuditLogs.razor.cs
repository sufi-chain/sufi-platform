using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;
using SufiChain.SufiAbp.AuditLogging.Dtos;
using SufiChain.SufiAbp.AuditLogging.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;

namespace SufiChain.SufiAbp.AuditLogging.Blazor.Pages;

public partial class AuditLogs : AuditLoggingComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadAuditLogs = "load-audit-logs";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IAuditLogAppService AuditLogAppService => LazyGetRequiredService(ref _auditLogAppService);
    private IAuditLogAppService? _auditLogAppService;

    private SbDataGrid<AuditLogListItemDto>? _gridRef;
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    // Filters
    private DateOnly? _startDate;
    private DateOnly? _endDate;
    private string? _userName;
    private string? _httpMethod;
    private string? _url;
    private string? _minDurationText;
    private string? _maxDurationText;
    private bool? _hasException;

    private int? MinDuration => int.TryParse(_minDurationText, out var val) ? val : null;
    private int? MaxDuration => int.TryParse(_maxDurationText, out var val) ? val : null;

    private bool _showDetailModal;
    private Guid? _selectedAuditLogId;

    protected override void OnInitialized()
    {
        SetupPageLayout();
        // Default to last 30 days to ensure records are visible
        _startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        _endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)); // Include today fully in UTC
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadAuditLogs);
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["AuditLogs"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    /// <summary>
    /// Server-side data provider for the audit log grid.
    /// Uses SbDataRequest for pagination; applies component filter state.
    /// </summary>
    private async Task<SbDataResponse<AuditLogListItemDto>> LoadAuditLogsDataAsync(SbDataRequest request)
    {
        var input = new GetAuditLogListInput
        {
            StartTime = _startDate.HasValue
                ? DateTime.SpecifyKind(_startDate.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : null,
            EndTime = _endDate.HasValue
                ? DateTime.SpecifyKind(_endDate.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc)
                : null,
            UserName = string.IsNullOrWhiteSpace(_userName) ? null : _userName,
            HttpMethod = _httpMethod,
            Url = string.IsNullOrWhiteSpace(_url) ? null : _url,
            MinExecutionDuration = MinDuration,
            MaxExecutionDuration = MaxDuration,
            HasException = _hasException,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize,
            Sorting = "ExecutionTime DESC"
        };

        var result = await AuditLogAppService.GetListAsync(input);
        return new SbDataResponse<AuditLogListItemDto>(result.Items, result.TotalCount);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
    }

    private async Task OnPageSizeChangedAsync(int pageSize)
    {
        _pageSize = pageSize;
        _pageIndex = 0;
    }

    private async Task ApplyFiltersAsync()
    {
        _pageIndex = 0;
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadAuditLogs);
    }

    private async Task ClearFiltersAsync()
    {
        _startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        _endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        _userName = null;
        _httpMethod = null;
        _url = null;
        _minDurationText = null;
        _maxDurationText = null;
        _hasException = null;
        _pageIndex = 0;
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadAuditLogs);
    }

    private void ShowDetailModal(AuditLogListItemDto auditLog)
    {
        _selectedAuditLogId = auditLog.Id;
        _showDetailModal = true;
    }

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
}
