using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Permissions;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Pages.AIManagement;

public partial class ModelConfigurations : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspaces = "load-workspaces";
        public const string LoadConfigurations = "load-configurations";
        public const string ToggleConfiguration = "toggle-configuration";
        public const string DeleteConfiguration = "delete-configuration";
    }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private List<WorkspaceDto> _workspaces = new();
    private List<AIModelConfigurationDto> _configurations = new();
    private Guid? _selectedWorkspaceId;
    private bool _modalOpen;
    private AIModelConfigurationDto? _editingConfiguration;

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
                await LoadConfigurationsAsync();
            }
        }, LoadingKeys.LoadWorkspaces);
    }

    private async Task OnWorkspaceChangedAsync(Guid? workspaceId)
    {
        _selectedWorkspaceId = workspaceId;
        if (_selectedWorkspaceId.HasValue)
        {
            await LoadConfigurationsAsync();
        }
        else
        {
            _configurations.Clear();
        }
    }

    private async Task LoadConfigurationsAsync()
    {
        if (!_selectedWorkspaceId.HasValue) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            _configurations = await AIAppService.GetModelConfigurationsAsync(_selectedWorkspaceId.Value);
        }, LoadingKeys.LoadConfigurations);
    }

    private void OpenCreateModal()
    {
        _editingConfiguration = null;
        _modalOpen = true;
    }

    private void OpenEditModal(AIModelConfigurationDto configuration)
    {
        _editingConfiguration = configuration;
        _modalOpen = true;
    }

    private async Task OnConfigurationSavedAsync()
    {
        _modalOpen = false;
        _editingConfiguration = null;
        await LoadConfigurationsAsync();
    }

    private async Task ToggleConfigurationAsync(AIModelConfigurationDto configuration)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var updateDto = new UpdateAIModelConfigurationDto
            {
                ModelId = configuration.ModelId,
                ApiEndpoint = configuration.ApiEndpoint,
                ConfigurationJson = configuration.ConfigurationJson,
                Priority = configuration.Priority
            };

            // Toggle the enabled state
            configuration.IsEnabled = !configuration.IsEnabled;

            await AIAppService.UpdateModelConfigurationAsync(configuration.Id, updateDto);
            await Message.SuccessAsync(L[configuration.IsEnabled ? "ConfigurationEnabled" : "ConfigurationDisabled"]);
        }, LoadingKeys.ToggleConfiguration);
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
}
