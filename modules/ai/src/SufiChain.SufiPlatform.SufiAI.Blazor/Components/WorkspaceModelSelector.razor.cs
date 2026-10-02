using System.Text.Json;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.UI.Browser;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceModelSelector : AIComponentBase
{
    [Parameter] public Guid WorkspaceId { get; set; }

    [Parameter] public Guid? ModelConfigurationId { get; set; }

    [Parameter] public EventCallback<Guid?> ModelConfigurationIdChanged { get; set; }

    [Parameter] public string? ReasoningEffort { get; set; }

    [Parameter] public EventCallback<string?> ReasoningEffortChanged { get; set; }

    [Parameter] public OpenAIApiMode ApiMode { get; set; } = OpenAIApiMode.ChatCompletions;

    [Parameter] public EventCallback<OpenAIApiMode> ApiModeChanged { get; set; }

    [Parameter] public EventCallback<AIModelRouteDto?> RouteChanged { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public bool ShowCatalogHints { get; set; } = true;

    [Parameter] public bool Compact { get; set; }

    [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;

    [Inject] private IAIModelCatalogAppService CatalogAppService { get; set; } = default!;

    private List<AIModelRouteDto> _routes = new();
    private Guid _loadedWorkspaceId;
    private Guid? _loadedTenantId;
    private bool _catalogLoaded;
    private bool _compactOpen;
    private bool _effortOpen;
    private bool _publishedRoute;
    private Guid _publishedRouteId;

    protected bool ShouldRenderPicker =>
        WorkspaceId != Guid.Empty &&
        _routes.Count > 1;

    protected bool CanChoose => ShouldRenderPicker && !Disabled;

    protected Guid SelectedRouteId =>
        ModelConfigurationId ?? _routes.FirstOrDefault(route => route.IsDefault)?.Id ?? Guid.Empty;

    private AIModelRouteDto? SelectedRoute =>
        _routes.FirstOrDefault(route => route.Id == SelectedRouteId)
        ?? (ModelConfigurationId == null
            ? _routes.FirstOrDefault(route => route.IsReady && route.IsDefault)
              ?? _routes.FirstOrDefault(route => route.IsReady)
              ?? (_routes.Count == 1 ? _routes[0] : null)
            : null);

    private string SelectedEffort =>
        ReasoningEffort ?? SelectedRoute?.DefaultReasoningEffort ?? EffortOptions.FirstOrDefault() ?? string.Empty;

    private string EffortTriggerTitle =>
        $"{L["ReasoningEffort"]}: {FormatEffort(SelectedEffort)}";

    private string EffortTriggerCssClass =>
        _effortOpen
            ? "workspace-model-selector__trigger workspace-model-selector__trigger--open"
            : "workspace-model-selector__trigger";

    private bool IsSelectedEffort(string effort) =>
        string.Equals(effort, SelectedEffort, StringComparison.OrdinalIgnoreCase);

    private string GetEffortOptionCssClass(string effort) =>
        IsSelectedEffort(effort)
            ? "workspace-model-selector__option workspace-model-selector__option--selected"
            : "workspace-model-selector__option";

    private static string FormatEffort(string effort)
    {
        if (string.IsNullOrWhiteSpace(effort))
        {
            return string.Empty;
        }

        var value = effort.Trim();
        return value.Equals("xhigh", StringComparison.OrdinalIgnoreCase)
            ? "Extra high"
            : char.ToUpperInvariant(value[0]) + value[1..];
    }

    private string CompactTriggerLabel =>
        SelectedRoute == null ? L["WorkspaceChat:ModelSelector"] : FormatRouteName(SelectedRoute);

    private string CompactTriggerTitle =>
        $"{L["WorkspaceChat:ModelSelector"]}: {CompactTriggerLabel}";

    private string CompactTriggerCssClass =>
        _compactOpen
            ? "workspace-model-selector__trigger workspace-model-selector__trigger--open"
            : "workspace-model-selector__trigger";

    protected override async Task OnParametersSetAsync()
    {
        if (WorkspaceId == Guid.Empty)
        {
            _routes = new List<AIModelRouteDto>();
            _catalogLoaded = false;
            _compactOpen = false;
            _effortOpen = false;
            await PublishSelectedRouteAsync();
            return;
        }

        var tenantId = CurrentTenant.Id;
        var identityChanged =
            !_catalogLoaded ||
            _loadedWorkspaceId != WorkspaceId ||
            _loadedTenantId != tenantId;

        if (!identityChanged)
        {
            await PublishSelectedRouteAsync();
            return;
        }

        _loadedWorkspaceId = WorkspaceId;
        _loadedTenantId = tenantId;
        await LoadCatalogAndSelectionAsync();
    }

    private async Task LoadCatalogAndSelectionAsync()
    {
        _publishedRoute = false;
        try
        {
            _routes = await CatalogAppService.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = WorkspaceId,
                CapabilityType = AICapabilityType.ChatCompletion
            });
        }
        catch
        {
            _routes = new List<AIModelRouteDto>();
            _catalogLoaded = true;
            await PublishSelectedRouteAsync();
            return;
        }

        _catalogLoaded = true;
        var stored = await ReadStoredSelectionAsync();
        var selected = stored?.RouteId is Guid storedId && _routes.Any(route => route.Id == storedId && route.IsUserSelectable)
            ? storedId
            : _routes.FirstOrDefault(route => route.IsReady && route.IsDefault)?.Id
              ?? _routes.FirstOrDefault(route => route.IsReady)?.Id;
        var selectableId = SelectableRouteId(selected);

        if (selectableId != ModelConfigurationId)
        {
            await ModelConfigurationIdChanged.InvokeAsync(selectableId);
        }

        await AlignEffortAsync(selected, stored?.ReasoningEffort);
        await AlignApiModeAsync(selected, stored?.ApiMode ?? OpenAIApiMode.ChatCompletions);
        await PublishRouteAsync(selected);
    }

    private Guid? SelectableRouteId(Guid? routeId)
    {
        if (routeId is not Guid id)
        {
            return null;
        }

        var route = _routes.FirstOrDefault(item => item.Id == id);
        return route is { IsUserSelectable: true } ? id : null;
    }

    private Task PublishSelectedRouteAsync() => PublishRouteAsync(SelectedRoute?.Id);

    private async Task PublishRouteAsync(Guid? routeId)
    {
        var route = routeId is Guid selectedId
            ? _routes.FirstOrDefault(item => item.Id == selectedId) ?? SelectedRoute
            : SelectedRoute;
        var publishedId = route?.Id ?? Guid.Empty;
        if (_publishedRoute && _publishedRouteId == publishedId)
        {
            return;
        }

        _publishedRoute = true;
        _publishedRouteId = publishedId;
        if (RouteChanged.HasDelegate)
        {
            await RouteChanged.InvokeAsync(route);
        }
    }

    private Task ToggleCompactAsync()
    {
        if (Disabled || !CanChoose)
        {
            return Task.CompletedTask;
        }

        _compactOpen = !_compactOpen;
        if (_compactOpen)
        {
            _effortOpen = false;
        }

        return Task.CompletedTask;
    }

    private Task ToggleEffortAsync()
    {
        if (Disabled || EffortOptions.Count == 0)
        {
            return Task.CompletedTask;
        }

        _effortOpen = !_effortOpen;
        if (_effortOpen)
        {
            _compactOpen = false;
        }

        return Task.CompletedTask;
    }

    private async Task OnCompactEffortChosenAsync(string effort)
    {
        _effortOpen = false;
        await OnEffortChangedAsync(effort);
    }

    private async Task OnCompactRouteChosenAsync(Guid routeId)
    {
        _compactOpen = false;
        await OnSelectedRouteChangedAsync(routeId);
    }

    private Task OnRouteChangedAsync(Guid routeId) => OnSelectedRouteChangedAsync(routeId);

    private static string RouteSearchText(AIModelRouteDto route) =>
        $"{route.DisplayName} {route.ModelId}";

    private async Task OnSelectedRouteChangedAsync(Guid routeId)
    {
        Guid? selected = routeId == Guid.Empty ? null : routeId;
        await ModelConfigurationIdChanged.InvokeAsync(selected);
        var effort = await AlignEffortAsync(selected, preferredEffort: null);
        var mode = await AlignApiModeAsync(selected, ApiMode);
        await WriteStoredSelectionAsync(selected, effort, mode);
        await PublishRouteAsync(selected);
    }

    private async Task OnEffortChangedAsync(string? value)
    {
        var effort = NormalizeEffort(SelectedRoute, value);
        await ReasoningEffortChanged.InvokeAsync(effort);
        await WriteStoredSelectionAsync(SelectedRoute?.Id, effort, ApiMode);
    }

    private async Task OnApiModeChangedAsync(OpenAIApiMode mode)
    {
        var selected = await AlignApiModeAsync(SelectedRoute?.Id, mode);
        await WriteStoredSelectionAsync(SelectedRoute?.Id, ReasoningEffort, selected);
    }

    private async Task<OpenAIApiMode> AlignApiModeAsync(Guid? routeId, OpenAIApiMode preferred)
    {
        var route = routeId is Guid selectedId
            ? _routes.FirstOrDefault(item => item.Id == selectedId)
            : SelectedRoute;
        var mode = route?.SupportsResponses == true ? preferred : OpenAIApiMode.ChatCompletions;
        if (mode != ApiMode)
        {
            await ApiModeChanged.InvokeAsync(mode);
        }

        return mode;
    }

    private async Task<string?> AlignEffortAsync(Guid? routeId, string? preferredEffort)
    {
        var route = routeId is Guid selectedId
            ? _routes.FirstOrDefault(item => item.Id == selectedId)
            : SelectedRoute;
        var effort = NormalizeEffort(route, preferredEffort ?? ReasoningEffort);
        if (effort != ReasoningEffort)
        {
            await ReasoningEffortChanged.InvokeAsync(effort);
        }

        return effort;
    }

    private static string? NormalizeEffort(AIModelRouteDto? route, string? effort)
    {
        var options = SplitEfforts(route);
        if (options.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(effort) && options.Contains(effort, StringComparer.OrdinalIgnoreCase))
        {
            return options.First(option => string.Equals(option, effort, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(route?.DefaultReasoningEffort) &&
            options.Contains(route.DefaultReasoningEffort, StringComparer.OrdinalIgnoreCase))
        {
            return route.DefaultReasoningEffort;
        }

        return options[0];
    }

    private IReadOnlyList<string> EffortOptions => SplitEfforts(SelectedRoute);

    private static IReadOnlyList<string> SplitEfforts(AIModelRouteDto? route)
    {
        if (route?.SupportsReasoning != true || string.IsNullOrWhiteSpace(route.ReasoningEfforts))
        {
            return Array.Empty<string>();
        }

        return route.ReasoningEfforts
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private string GetCompactOptionCssClass(AIModelRouteDto route)
    {
        return route.Id == SelectedRouteId
            ? "workspace-model-selector__option workspace-model-selector__option--selected"
            : "workspace-model-selector__option";
    }

    private static string FormatRouteName(AIModelRouteDto route) =>
        string.IsNullOrWhiteSpace(route.DisplayName) ? route.ModelId : route.DisplayName;

    private string FormatRouteMeta(AIModelRouteDto route)
    {
        var parts = new List<string>();
        if (route.AcceptsImageInput == true)
        {
            parts.Add(L["AcceptsImageInput"]);
        }

        if (route.AcceptsFileInput == true)
        {
            parts.Add(L["AcceptsFileInput"]);
        }

        if (route.SupportsReasoning == true)
        {
            parts.Add(L["SupportsReasoning"]);
        }

        if (route.IsDefault)
        {
            parts.Add(L["Default"]);
        }

        if (!route.IsReady && !string.IsNullOrWhiteSpace(route.UnavailableReason))
        {
            parts.Add(route.UnavailableReason);
        }

        return string.Join(" · ", parts);
    }

    private string FormatRouteLabel(AIModelRouteDto route)
    {
        var label = FormatRouteName(route);
        if (route.IsDefault)
        {
            label = $"{label} ({L["Default"]})";
        }

        if (!route.IsReady && !string.IsNullOrWhiteSpace(route.UnavailableReason))
        {
            label = $"{label} — {route.UnavailableReason}";
        }

        return label;
    }

    private string BuildStorageKey()
    {
        var tenantKey = CurrentTenant.Id?.ToString("N") ?? "host";
        return $"sufiai.workspace-chat.model:{tenantKey}:{WorkspaceId:N}";
    }

    private async Task<StoredSelection?> ReadStoredSelectionAsync()
    {
        var json = await SessionStorage.GetItemAsync(BuildStorageKey());
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<StoredSelection>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Task WriteStoredSelectionAsync(Guid? routeId, string? effort, OpenAIApiMode apiMode)
    {
        var json = JsonSerializer.Serialize(new StoredSelection
        {
            RouteId = routeId,
            ReasoningEffort = effort,
            ApiMode = apiMode
        });
        return SessionStorage.SetItemAsync(BuildStorageKey(), json).AsTask();
    }

    private sealed class StoredSelection
    {
        public Guid? RouteId { get; set; }

        public string? ReasoningEffort { get; set; }

        public OpenAIApiMode ApiMode { get; set; }
    }
}
