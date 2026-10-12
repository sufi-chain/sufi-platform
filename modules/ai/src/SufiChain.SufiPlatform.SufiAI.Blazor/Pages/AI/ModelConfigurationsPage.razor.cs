using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class ModelConfigurationsPage : AIComponentBase
{
    private enum LoadState
    {
        NotStarted,
        Loading,
        Loaded,
        NotFound,
        Failed
    }

    private static class LoadingKeys
    {
        public const string LoadWorkspace = "load-workspace";
        public const string LoadConfigurations = "load-configurations";
        public const string DeleteConfiguration = "delete-configuration";
        public const string SetWorkspaceDefault = "set-workspace-default";
    }

    [Parameter]
    public Guid WorkspaceId { get; set; }

    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private WorkspaceDto? _workspace;
    private List<AIModelConfigurationDto> _configurations = new();
    private bool _readOnly;
    private Guid _loadedWorkspaceId;
    private LoadState _loadState = LoadState.NotStarted;
    private int _loadVersion;

    private bool CanCreateConfiguration =>
        _loadState == LoadState.Loaded && _workspace != null && !_readOnly;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["ModelConfigurations"];
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!IsInteractive || SameLoad())
        {
            return;
        }

        await LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await LoadAsync();
        }
    }

    private bool SameLoad() =>
        _loadState == LoadState.Loaded && WorkspaceId == _loadedWorkspaceId;

    private async Task LoadAsync()
    {
        if (SameLoad())
        {
            return;
        }

        var version = ++_loadVersion;
        var workspaceId = WorkspaceId;
        _loadState = LoadState.Loading;
        _workspace = null;
        _configurations = new List<AIModelConfigurationDto>();
        _readOnly = false;

        if (workspaceId == Guid.Empty)
        {
            _loadState = LoadState.NotFound;
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            if (version != _loadVersion)
            {
                return;
            }

            try
            {
                _workspace = await WorkspaceAppService.GetAsync(workspaceId);
            }
            catch (OperationCanceledException exception)
            {
                Logger.LogWarning(
                    exception,
                    "Model configurations workspace load was canceled. WorkspaceId: {WorkspaceId}",
                    workspaceId);
                throw;
            }
            catch (Exception exception) when (IsEntityNotFound(exception))
            {
                if (version != _loadVersion)
                {
                    return;
                }

                _workspace = null;
                _loadState = LoadState.NotFound;
                return;
            }
            catch (Exception exception)
            {
                if (version != _loadVersion)
                {
                    return;
                }

                Logger.LogWarning(
                    exception,
                    "Model configurations workspace load failed. WorkspaceId: {WorkspaceId}",
                    workspaceId);
                _workspace = null;
                _loadState = LoadState.Failed;
                return;
            }

            if (version != _loadVersion || _workspace == null)
            {
                return;
            }

            _readOnly = _workspace.IsInherited;
            PageLayout.Title = string.IsNullOrWhiteSpace(_workspace.Name)
                ? L["ModelConfigurations"]
                : $"{L["ModelConfigurations"]} — {_workspace.Name}";

            try
            {
                _configurations = await AIAppService.GetModelConfigurationsAsync(workspaceId);
            }
            catch (OperationCanceledException exception)
            {
                Logger.LogWarning(
                    exception,
                    "Model configurations list load was canceled. WorkspaceId: {WorkspaceId}",
                    workspaceId);
                throw;
            }
            catch (Exception exception)
            {
                if (version != _loadVersion)
                {
                    return;
                }

                Logger.LogWarning(
                    exception,
                    "Model configurations list load failed. WorkspaceId: {WorkspaceId}",
                    workspaceId);
                _loadState = LoadState.Failed;
                return;
            }

            if (version != _loadVersion)
            {
                return;
            }

            _loadState = LoadState.Loaded;
            _loadedWorkspaceId = workspaceId;
        }, LoadingKeys.LoadWorkspace);
    }

    private Task RetryLoadAsync()
    {
        _loadedWorkspaceId = Guid.Empty;
        _loadState = LoadState.NotStarted;
        return LoadAsync();
    }

    private void BackToWorkspaces()
    {
        Navigation.NavigateTo("/panel/admin/ai/workspaces");
    }

    private void OpenCreatePage()
    {
        if (!CanCreateConfiguration)
        {
            return;
        }

        Navigation.NavigateTo($"/panel/admin/ai/workspaces/{WorkspaceId}/model-configurations/new");
    }

    private void OpenEditPage(AIModelConfigurationDto configuration)
    {
        if (_readOnly)
        {
            return;
        }

        Navigation.NavigateTo($"/panel/admin/ai/workspaces/{WorkspaceId}/model-configurations/{configuration.Id}");
    }

    private bool IsWorkspaceDefault(AIModelConfigurationDto configuration)
    {
        if (!configuration.IsEnabled || configuration.CapabilityType != AICapabilityType.ChatCompletion)
        {
            return false;
        }

        return !_configurations.Any(item =>
            item.Id != configuration.Id
            && item.IsEnabled
            && item.CapabilityType == AICapabilityType.ChatCompletion
            && item.Priority < configuration.Priority);
    }

    private bool CanSetWorkspaceDefault(AIModelConfigurationDto configuration)
    {
        return !_readOnly
            && configuration.IsEnabled
            && configuration.CapabilityType == AICapabilityType.ChatCompletion
            && !IsWorkspaceDefault(configuration);
    }

    private async Task SetWorkspaceDefaultAsync(AIModelConfigurationDto configuration)
    {
        if (!CanSetWorkspaceDefault(configuration))
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(
            L["SetAsWorkspaceDefaultConfirmation", FormatModelLabel(configuration)],
            L["SetAsWorkspaceDefault"]);
        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await AIAppService.SetWorkspaceDefaultModelConfigurationAsync(configuration.Id);
            await Message.SuccessAsync(L["WorkspaceDefaultUpdated"]);
            await ReloadConfigurationsAsync();
        }, LoadingKeys.SetWorkspaceDefault);
    }

    private async Task DeleteConfigurationAsync(AIModelConfigurationDto configuration)
    {
        if (_readOnly || IsWorkspaceDefault(configuration))
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(L["DeleteConfigurationConfirmation", configuration.ModelId]);
        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await AIAppService.DeleteModelConfigurationAsync(configuration.Id);
            await Message.SuccessAsync(L["ConfigurationDeleted"]);
            await ReloadConfigurationsAsync();
        }, LoadingKeys.DeleteConfiguration);
    }

    private async Task ReloadConfigurationsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            _configurations = await AIAppService.GetModelConfigurationsAsync(WorkspaceId);
        }, LoadingKeys.LoadConfigurations);
    }

    private SbColor GetCapabilityColor(AICapabilityType capabilityType)
    {
        return capabilityType switch
        {
            AICapabilityType.ChatCompletion => SbColor.Primary,
            AICapabilityType.AudioTranscription => SbColor.Info,
            AICapabilityType.TextToSpeech => SbColor.Success,
            AICapabilityType.VisionAnalysis => SbColor.Warning,
            AICapabilityType.Embeddings => SbColor.Secondary,
            AICapabilityType.ImageGeneration => SbColor.Danger,
            _ => SbColor.Default
        };
    }

    private string FormatCapability(AICapabilityType capabilityType)
    {
        var key = capabilityType.ToString();
        var localized = L[key];
        return localized.ResourceNotFound || string.Equals(localized.Value, key, StringComparison.Ordinal)
            ? key
            : localized.Value;
    }

    private string FormatRoutePrice(decimal? value, AIPriceUnit unit)
    {
        if (!value.HasValue)
        {
            return unit == AIPriceUnit.PerMillionTokens ? L["InheritWorkspace"] : L["NotPriced"];
        }

        return value.Value.ToString("0.####", CultureInfo.InvariantCulture) + " " + PriceSuffix(unit);
    }

    private string FormatOutputPrice(AIModelConfigurationDto route)
    {
        if (!route.OutputPrice.HasValue && route.InputPriceUnit != AIPriceUnit.PerMillionTokens)
        {
            return L["NotPriced"];
        }

        return FormatRoutePrice(route.OutputPrice, route.OutputPriceUnit);
    }

    private string PriceSuffix(AIPriceUnit unit) => unit switch
    {
        AIPriceUnit.PerMinute => L["PriceSuffixMinute"],
        AIPriceUnit.PerHour => L["PriceSuffixHour"],
        AIPriceUnit.PerMillionCharacters => L["PriceSuffixMillionCharacters"],
        AIPriceUnit.PerImage => L["PriceSuffixImage"],
        AIPriceUnit.PerRequest => L["PriceSuffixRequest"],
        _ => L["PriceSuffixMillionTokens"]
    };

    private static string FormatModelLabel(AIModelConfigurationDto route) =>
        string.IsNullOrWhiteSpace(route.DisplayName) ? route.ModelId : route.DisplayName;

    private string FormatChatFlags(AIModelConfigurationDto route)
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

        return string.Join(" · ", parts);
    }

    private static bool IsEntityNotFound(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().Name, "EntityNotFoundException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
