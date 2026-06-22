using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiAbp.AI.Blazor.Components;

public partial class ModelConfigurationsModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadConfigurations = "load-configurations";
        public const string DeleteConfiguration = "delete-configuration";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public string? WorkspaceName { get; set; }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private List<AIModelConfigurationDto> _configurations = new();
    private bool _modelConfigurationModalOpen;
    private AIModelConfigurationDto? _editingConfiguration;
    private bool _wasOpen;

    protected override async Task OnParametersSetAsync()
    {
        if (Open && !_wasOpen)
        {
            await LoadConfigurationsAsync();
        }

        if (!Open)
        {
            _configurations.Clear();
            _editingConfiguration = null;
            _modelConfigurationModalOpen = false;
        }

        _wasOpen = Open;
    }

    private async Task LoadConfigurationsAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            _configurations.Clear();
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _configurations = await AIAppService.GetModelConfigurationsAsync(WorkspaceId.Value);
        }, LoadingKeys.LoadConfigurations);
    }

    private void OpenCreateModal()
    {
        _editingConfiguration = null;
        _modelConfigurationModalOpen = true;
    }

    private void OpenEditModal(AIModelConfigurationDto configuration)
    {
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

    private string GetTitle()
    {
        return string.IsNullOrWhiteSpace(WorkspaceName)
            ? L["ModelConfigurations"]
            : $"{L["ModelConfigurations"]} - {WorkspaceName}";
    }

    private async Task CloseAsync()
    {
        await SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }
}
