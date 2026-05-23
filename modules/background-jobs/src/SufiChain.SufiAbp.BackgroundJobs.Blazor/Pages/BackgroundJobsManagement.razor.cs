using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiAbp.BackgroundJobs.Dtos;
using SufiChain.SufiAbp.BackgroundJobs.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.BackgroundJobs;

namespace SufiChain.SufiAbp.BackgroundJobs.Blazor.Pages;

public partial class BackgroundJobsManagement : BackgroundJobsComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadJobs = "load-jobs";
        public const string DeleteJob = "delete-job";
        public const string RetryJob = "retry-job";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    
    private IBackgroundJobAppService BackgroundJobAppService => LazyGetRequiredService(ref _backgroundJobAppService);
    private IBackgroundJobAppService? _backgroundJobAppService;

    private List<BackgroundJobListItemDto> _jobs = new();
    private int _pageIndex = 0;
    private int _pageSize = 20;
    private long _totalCount;

    // Filters
    private string? _jobName;
    private string? _applicationName;
    private bool? _isAbandoned;
    private BackgroundJobPriority? _priority;

    // Filter helpers
    private bool HasActiveFilters => !string.IsNullOrWhiteSpace(_jobName) 
        || !string.IsNullOrWhiteSpace(_applicationName) 
        || _isAbandoned.HasValue 
        || _priority.HasValue;

    private int ActiveFilterCount
    {
        get
        {
            var count = 0;
            if (!string.IsNullOrWhiteSpace(_jobName)) count++;
            if (!string.IsNullOrWhiteSpace(_applicationName)) count++;
            if (_isAbandoned.HasValue) count++;
            if (_priority.HasValue) count++;
            return count;
        }
    }

    private bool _showDetailModal;
    private BackgroundJobListItemDto? _selectedJob;

    // Confirmation dialog state
    private bool _showDeleteConfirm;
    private BackgroundJobListItemDto? _jobToDelete;
    
    private bool _showAbandonConfirm;
    private BackgroundJobListItemDto? _jobToAbandon;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadJobsAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["BackgroundJobs"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadJobsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new GetBackgroundJobListInput
        {
            JobName = _jobName,
            ApplicationName = _applicationName,
            IsAbandoned = _isAbandoned,
            Priority = _priority,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize
        };

        var result = await BackgroundJobAppService.GetListAsync(input);
        _jobs = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadJobs);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadJobsAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        _pageIndex = 0;
        await LoadJobsAsync();
    }

    private async Task ClearFiltersAsync()
    {
        _jobName = null;
        _applicationName = null;
        _isAbandoned = null;
        _priority = null;
        _pageIndex = 0;
        await LoadJobsAsync();
    }

    private async Task HandleFilterKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await ApplyFiltersAsync();
        }
    }

    private async Task OnPriorityChanged(BackgroundJobPriority? value)
    {
        _priority = value;
        await ApplyFiltersAsync();
    }

    private async Task OnStatusChanged(bool? value)
    {
        _isAbandoned = value;
        await ApplyFiltersAsync();
    }

    private async Task ToggleQuickFilter(string filter)
    {
        switch (filter)
        {
            case "abandoned":
                _isAbandoned = _isAbandoned == true ? null : true;
                _priority = null;
                break;
            case "high":
                _priority = _priority == BackgroundJobPriority.High ? null : BackgroundJobPriority.High;
                _isAbandoned = null;
                break;
            case "active":
                _isAbandoned = _isAbandoned == false ? null : false;
                _priority = null;
                break;
        }
        await ApplyFiltersAsync();
    }

    private void ShowDetailModal(BackgroundJobListItemDto job)
    {
        _selectedJob = job;
        _showDetailModal = true;
    }

    private void ShowDeleteConfirm(BackgroundJobListItemDto job)
    {
        _jobToDelete = job;
        _showDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        _showDeleteConfirm = false;
        _jobToDelete = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (_jobToDelete == null) return;
        
        var job = _jobToDelete;
        _showDeleteConfirm = false;
        _jobToDelete = null;

        await ExecuteWithLoadingAsync(async () =>
        {
            await BackgroundJobAppService.DeleteAsync(job.Id);
            await Notify.SuccessAsync(L["JobDeletedSuccessfully"]);
            await LoadJobsAsync();
        }, LoadingKeys.DeleteJob);
    }

    private async Task RetryJobAsync(BackgroundJobListItemDto job)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await BackgroundJobAppService.RetryAsync(job.Id);
            await Notify.SuccessAsync(L["JobRetriedSuccessfully"]);
            await LoadJobsAsync();
        }, LoadingKeys.RetryJob);
    }

    private void ShowAbandonConfirm(BackgroundJobListItemDto job)
    {
        _jobToAbandon = job;
        _showAbandonConfirm = true;
    }

    private void CancelAbandon()
    {
        _showAbandonConfirm = false;
        _jobToAbandon = null;
    }

    private async Task ConfirmAbandonAsync()
    {
        if (_jobToAbandon == null) return;
        
        var job = _jobToAbandon;
        _showAbandonConfirm = false;
        _jobToAbandon = null;

        await ExecuteWithLoadingAsync(async () =>
        {
            await BackgroundJobAppService.AbandonAsync(job.Id);
            await Notify.SuccessAsync(L["JobAbandonedSuccessfully"]);
            await LoadJobsAsync();
        }, LoadingKeys.DeleteJob);
    }

    private SbColor GetStatusColor(BackgroundJobListItemDto job)
    {
        if (job.IsAbandoned) return SbColor.Danger;
        if (job.TryCount > 3) return SbColor.Warning;
        if (job.NextTryTime.HasValue && job.NextTryTime.Value <= DateTime.Now) return SbColor.Info;
        return SbColor.Success;
    }

    private string GetStatusText(BackgroundJobListItemDto job)
    {
        if (job.IsAbandoned) return L["Abandoned"];
        if (job.TryCount > 3) return L["Retrying"];
        if (job.NextTryTime.HasValue && job.NextTryTime.Value <= DateTime.Now) return L["Pending"];
        return L["Scheduled"];
    }

    private SbColor GetPriorityColor(BackgroundJobPriority priority)
    {
        return priority switch
        {
            BackgroundJobPriority.High => SbColor.Danger,
            BackgroundJobPriority.AboveNormal => SbColor.Warning,
            BackgroundJobPriority.Normal => SbColor.Primary,
            BackgroundJobPriority.BelowNormal => SbColor.Info,
            BackgroundJobPriority.Low => SbColor.Secondary,
            _ => SbColor.Default
        };
    }
}
