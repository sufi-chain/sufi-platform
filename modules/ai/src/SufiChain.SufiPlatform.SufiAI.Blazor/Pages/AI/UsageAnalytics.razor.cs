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

    private List<WorkspaceDto> _workspaces = new();
    private bool _workspacesResolved;
    private bool _workspacesFailed;
    private Guid? _selectedWorkspaceId;
    private UsageStatisticsDto? _statistics;
    private bool _statisticsFailed;
    private bool _logsFailed;
    private List<AIUsageLogDto> _usageLogs = new();
    private List<WorkspaceGuardrailStatusDto> _guardrailStatus = new();
    private string _routeFilter = AllRoutesFilter;

    private const string AllRoutesFilter = "all";
    private const string UnattributedRouteFilter = "unattributed";

    private SbDateRange? _dateRange = new(DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), DateOnly.FromDateTime(DateTime.Now));

    protected override void OnInitialized()
    {
        LoadingStates[LoadingKeys.LoadWorkspaces] = true;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadWorkspacesAsync();
    }

    private Task LoadWorkspacesAsync()
    {
        _workspacesFailed = false;
        if (_workspaces.Count == 0)
        {
            _workspacesResolved = false;
        }

        return ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _workspaces = await WorkspaceAppService.GetLookupAsync();
                _workspacesFailed = false;

                if (_workspaces.Any() && !_selectedWorkspaceId.HasValue)
                {
                    _selectedWorkspaceId = _workspaces.First().Id;
                    await LoadDataAsync();
                }
            }
            catch
            {
                _workspacesFailed = true;
            }
            finally
            {
                _workspacesResolved = true;
            }
        }, LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        _routeFilter = AllRoutesFilter;
        if (_selectedWorkspaceId.HasValue)
        {
            await LoadDataAsync();
        }
        else
        {
            _statistics = null;
            _usageLogs.Clear();
            _guardrailStatus.Clear();
            _routeFilter = AllRoutesFilter;
        }
    }

    private Task RetryAnalyticsAsync() => _workspacesFailed ? LoadWorkspacesAsync() : LoadDataAsync();

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

        _statisticsFailed = false;
        _logsFailed = false;
        LoadingStates[LoadingKeys.LoadUsageLogs] = true;
        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _statistics = await AIAppService.GetUsageStatisticsAsync(_selectedWorkspaceId.Value, startDate, endDate);
            }
            catch
            {
                _statisticsFailed = true;
                LoadingStates.TryRemove(LoadingKeys.LoadUsageLogs, out _);
            }
        }, LoadingKeys.LoadStatistics);

        if (_statisticsFailed || !_selectedWorkspaceId.HasValue)
        {
            LoadingStates.TryRemove(LoadingKeys.LoadUsageLogs, out _);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                var logs = await AIAppService.GetUsageLogsAsync(_selectedWorkspaceId.Value, startDate, endDate);
                _usageLogs = logs.OrderByDescending(x => x.CreationTime).Take(100).ToList();
                _logsFailed = false;
            }
            catch
            {
                _logsFailed = true;
            }
        }, LoadingKeys.LoadUsageLogs);

        try
        {
            _guardrailStatus = await WorkspaceAppService.GetGuardrailStatusAsync(_selectedWorkspaceId.Value);
        }
        catch
        {
            // Keep the counters and logs that already loaded. Guardrails are secondary.
        }
    }

    private static double GetGuardrailPercent(WorkspaceGuardrailStatusDto status)
    {
        if (status.LimitUsd <= 0)
        {
            return status.IsExceeded ? 100 : 0;
        }

        return Math.Min(100, Math.Max(0, (double)(status.UsedUsd / status.LimitUsd) * 100));
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

    private IEnumerable<UsageByRouteDto> FilteredCostByRoute
    {
        get
        {
            if (_statistics == null)
            {
                return Array.Empty<UsageByRouteDto>();
            }

            return _statistics.CostByRoute.Where(MatchesRouteFilter);
        }
    }

    private IReadOnlyList<AIUsageLogDto> VisibleUsageLogs =>
        _usageLogs.Where(MatchesLogFilter).ToList();

    private Task OnRouteFilterChangedAsync(string value)
    {
        _routeFilter = string.IsNullOrWhiteSpace(value) ? AllRoutesFilter : value;
        return Task.CompletedTask;
    }

    private bool MatchesRouteFilter(UsageByRouteDto route)
    {
        if (_routeFilter == AllRoutesFilter)
        {
            return true;
        }

        if (_routeFilter == UnattributedRouteFilter)
        {
            return route.IsUnattributed;
        }

        return route.ModelConfigurationId.HasValue
            && string.Equals(route.ModelConfigurationId.Value.ToString(), _routeFilter, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesLogFilter(AIUsageLogDto log)
    {
        if (_routeFilter == AllRoutesFilter)
        {
            return true;
        }

        if (_routeFilter == UnattributedRouteFilter)
        {
            return !log.ModelConfigurationId.HasValue;
        }

        return log.ModelConfigurationId.HasValue
            && string.Equals(log.ModelConfigurationId.Value.ToString(), _routeFilter, StringComparison.OrdinalIgnoreCase);
    }

    private string FormatRouteLabel(UsageByRouteDto route)
    {
        return route.IsUnattributed
            ? L["UnattributedRoute"]
            : (string.IsNullOrWhiteSpace(route.DisplayName) ? route.ModelId : route.DisplayName);
    }

    private string FormatLogRouteName(AIUsageLogDto log)
    {
        if (!log.ModelConfigurationId.HasValue)
        {
            return L["UnattributedRoute"];
        }

        return string.IsNullOrWhiteSpace(log.RouteDisplayName) ? log.ModelId : log.RouteDisplayName;
    }

    private string FormatCapability(AICapabilityType capabilityType)
    {
        var key = capabilityType.ToString();
        var localized = L[key];
        return localized.ResourceNotFound || string.Equals(localized.Value, key, StringComparison.Ordinal)
            ? key
            : localized.Value;
    }

    private string FormatUsageError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage)
            || errorMessage.StartsWith("Exception of type '", StringComparison.Ordinal))
        {
            return L["UnknownError"];
        }

        var localized = L[errorMessage];
        if (!localized.ResourceNotFound && !string.Equals(localized.Value, errorMessage, StringComparison.Ordinal))
        {
            return localized.Value;
        }

        return errorMessage;
    }

    private static string GetModelConfigurationsUrl(Guid workspaceId) =>
        $"/panel/admin/ai/workspaces/{workspaceId}/model-configurations";
}
