using System;
using System.Collections.Generic;
using System.Globalization;
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

    private async Task DeleteConfigurationAsync(AIModelConfigurationDto configuration)
    {
        if (_readOnly)
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

    private string FormatApiMode(AIModelConfigurationDto configuration)
    {
        if (configuration.CapabilityType is not (AICapabilityType.ChatCompletion or AICapabilityType.VisionAnalysis))
        {
            return L["NotApplicable"];
        }

        return configuration.OpenAIApiMode == OpenAIApiMode.Responses
            ? L["Responses"]
            : L["ChatCompletions"];
    }

    private string FormatRouteCost(decimal? value)
    {
        if (!value.HasValue)
        {
            return L["InheritWorkspace"];
        }

        return value.Value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
