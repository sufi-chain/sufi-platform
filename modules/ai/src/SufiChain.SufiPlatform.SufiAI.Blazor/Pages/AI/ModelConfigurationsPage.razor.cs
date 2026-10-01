using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class ModelConfigurationsPage : AIComponentBase
{
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
    private bool _modelConfigurationModalOpen;
    private AIModelConfigurationDto? _editingConfiguration;
    private bool _readOnly;
    private Guid _loadedWorkspaceId;

    protected override void OnInitialized()
    {
        PageLayout.Title = L["ModelConfigurations"];
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!IsInteractive || WorkspaceId == _loadedWorkspaceId)
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

    private async Task LoadAsync()
    {
        var workspaceId = WorkspaceId;
        await ExecuteWithLoadingAsync(async () =>
        {
            _workspace = await WorkspaceAppService.GetAsync(workspaceId);
            _readOnly = _workspace.IsInherited;
            PageLayout.Title = string.IsNullOrWhiteSpace(_workspace.Name)
                ? L["ModelConfigurations"]
                : $"{L["ModelConfigurations"]} — {_workspace.Name}";
        }, LoadingKeys.LoadWorkspace);

        await LoadConfigurationsAsync();
        _loadedWorkspaceId = workspaceId;
    }

    private async Task LoadConfigurationsAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            _configurations = await AIAppService.GetModelConfigurationsAsync(WorkspaceId);
        }, LoadingKeys.LoadConfigurations);
    }

    private void BackToWorkspaces()
    {
        Navigation.NavigateTo("/panel/admin/ai/workspaces");
    }

    private void OpenCreateModal()
    {
        if (_readOnly)
        {
            return;
        }

        _editingConfiguration = null;
        _modelConfigurationModalOpen = true;
    }

    private void OpenEditModal(AIModelConfigurationDto configuration)
    {
        if (_readOnly)
        {
            return;
        }

        _editingConfiguration = configuration;
        _modelConfigurationModalOpen = true;
    }

    private async Task OnConfigurationSavedAsync()
    {
        _modelConfigurationModalOpen = false;
        _editingConfiguration = null;
        await LoadConfigurationsAsync();
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
            await LoadConfigurationsAsync();
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
            await LoadConfigurationsAsync();
        }, LoadingKeys.DeleteConfiguration);
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
}
