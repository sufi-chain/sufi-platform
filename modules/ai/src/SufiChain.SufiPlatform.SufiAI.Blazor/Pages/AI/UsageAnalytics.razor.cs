using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiBlazor.Utilities.DateUtils;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class UsageAnalytics : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string LoadStatistics = "load-statistics";
        public const string LoadUsageLogs = "load-usage-logs";
    }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private List<WorkspaceDto> _workspaces = new();
    private Guid? _selectedWorkspaceId;
    private UsageStatisticsDto? _statistics;
    private List<AIUsageLogDto> _usageLogs = new();

    private SbDateRange? _dateRange = new(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DateOnly.FromDateTime(DateTime.Now));

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadWorkspacesAsync();
    }

    private async Task LoadWorkspacesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await WorkspaceAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000
            });
            _workspaces = result.Items.ToList();

            if (_workspaces.Any() && !_selectedWorkspaceId.HasValue)
            {
                _selectedWorkspaceId = _workspaces.First().Id;
                await LoadDataAsync();
            }
        }, LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        if (_selectedWorkspaceId.HasValue)
        {
            await LoadDataAsync();
        }
        else
        {
            _statistics = null;
            _usageLogs.Clear();
        }
    }

    private async Task RefreshDataAsync()
    {
        if (_selectedWorkspaceId.HasValue)
        {
            await LoadDataAsync();
        }
    }

    private async Task OnDateRangeChangedAsync(SbDateRange? dateRange)
    {
        _dateRange = dateRange;

        if (_selectedWorkspaceId.HasValue)
        {
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        if (!_selectedWorkspaceId.HasValue) return;

        var startDate = _dateRange?.Start?.ToDateTime(TimeOnly.MinValue);
        var endDate = _dateRange?.End?.ToDateTime(TimeOnly.MaxValue);

        await ExecuteWithLoadingAsync(async () =>
        {
            _statistics = await AIAppService.GetUsageStatisticsAsync(_selectedWorkspaceId.Value, startDate, endDate);
        }, LoadingKeys.LoadStatistics);

        await ExecuteWithLoadingAsync(async () =>
        {
            _usageLogs = await AIAppService.GetUsageLogsAsync(_selectedWorkspaceId.Value, startDate, endDate);
            // Show most recent first
            _usageLogs = _usageLogs.OrderByDescending(x => x.CreationTime).Take(100).ToList();
        }, LoadingKeys.LoadUsageLogs);
    }

    private string GetSuccessRate()
    {
        if (_statistics == null || _statistics.TotalRequests == 0)
        {
            return "0.0";
        }

        var rate = (_statistics.SuccessfulRequests / (double)_statistics.TotalRequests) * 100;
        return rate.ToString("F1");
    }

    private double GetPercentage(int value, int total)
    {
        if (total == 0) return 0;
        return (value / (double)total) * 100;
    }

    private double GetPercentage(double value, double total)
    {
        if (total == 0) return 0;
        return (value / total) * 100;
    }
}
