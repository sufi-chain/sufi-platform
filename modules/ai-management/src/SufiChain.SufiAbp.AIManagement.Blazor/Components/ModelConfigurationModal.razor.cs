using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Components;

public partial class ModelConfigurationModal : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string SaveConfiguration = "save-configuration";
        public const string LoadModels = "load-models";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public AIModelConfigurationDto? Configuration { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;
    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private bool _isEditMode;
    private CreateAIModelConfigurationDto _model = new();
    private string _priorityText = "0";
    private string _openAIApiModeText = string.Empty;
    private string _inputCostPer1KTokensText = string.Empty;
    private string _outputCostPer1KTokensText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private int _activeTab;
    private bool _wasOpen;

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            _isEditMode = Configuration != null;
            ResetForm();
        }

        _wasOpen = Open;
    }

    private void ResetForm()
    {
        if (_isEditMode && Configuration != null)
        {
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = Configuration.WorkspaceId,
                CapabilityType = Configuration.CapabilityType,
                ModelId = Configuration.ModelId,
                ApiEndpoint = Configuration.ApiEndpoint,
                ConfigurationJson = Configuration.ConfigurationJson,
                OpenAIApiMode = Configuration.OpenAIApiMode,
                InputCostPer1KTokens = Configuration.InputCostPer1KTokens,
                OutputCostPer1KTokens = Configuration.OutputCostPer1KTokens,
                Priority = Configuration.Priority
            };
            _priorityText = Configuration.Priority.ToString();
            _openAIApiModeText = Configuration.OpenAIApiMode?.ToString() ?? string.Empty;
            _inputCostPer1KTokensText = Configuration.InputCostPer1KTokens?.ToString() ?? string.Empty;
            _outputCostPer1KTokensText = Configuration.OutputCostPer1KTokens?.ToString() ?? string.Empty;
            _availableModels = new List<OpenAIModelDto>
            {
                new() { Id = Configuration.ModelId }
            };
            _activeTab = 0;
        }
        else
        {
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = WorkspaceId ?? Guid.Empty,
                CapabilityType = AICapabilityType.ChatCompletion,
                Priority = 0
            };
            _priorityText = "0";
            _openAIApiModeText = string.Empty;
            _inputCostPer1KTokensText = string.Empty;
            _outputCostPer1KTokensText = string.Empty;
            _availableModels = new List<OpenAIModelDto>();
            _activeTab = 0;
        }
    }

    private async Task SaveConfigurationAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            await Message.ErrorAsync(L["WorkspaceRequired"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.ModelId))
        {
            await Message.ErrorAsync(L["ModelIdRequired"]);
            return;
        }

        // Parse priority
        if (int.TryParse(_priorityText, out var priority))
        {
            _model.Priority = priority;
        }
        else
        {
            await Message.ErrorAsync(L["PriorityMustBeNumber"]);
            return;
        }

        // Validate JSON if provided
        if (!string.IsNullOrWhiteSpace(_model.ConfigurationJson))
        {
            try
            {
                JsonDocument.Parse(_model.ConfigurationJson);
            }
            catch
            {
                await Message.ErrorAsync(L["InvalidJsonFormat"]);
                return;
            }
        }

        _model.WorkspaceId = WorkspaceId.Value;
        if (!TryApplyOpenAIApiMode() || !TryApplyPricing())
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            if (_isEditMode && Configuration != null)
            {
                var updateDto = new UpdateAIModelConfigurationDto
                {
                    ModelId = _model.ModelId,
                    ApiEndpoint = _model.ApiEndpoint,
                    ApiKey = _model.ApiKey,
                    ConfigurationJson = _model.ConfigurationJson,
                    OpenAIApiMode = _model.OpenAIApiMode,
                    InputCostPer1KTokens = _model.InputCostPer1KTokens,
                    OutputCostPer1KTokens = _model.OutputCostPer1KTokens,
                    Priority = _model.Priority
                };
                await AIAppService.UpdateModelConfigurationAsync(Configuration.Id, updateDto);
                await Message.SuccessAsync(L["ConfigurationUpdated"]);
            }
            else
            {
                await AIAppService.CreateModelConfigurationAsync(_model);
                await Message.SuccessAsync(L["ConfigurationCreated"]);
            }

            await CloseModalAsync();
            await OnSaved.InvokeAsync();
        }, LoadingKeys.SaveConfiguration);
    }

    private async Task LoadModelsAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            await Message.ErrorAsync(L["WorkspaceRequired"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
            {
                WorkspaceId = WorkspaceId.Value,
                ApiKey = _model.ApiKey,
                ApiBaseUrl = _model.ApiEndpoint
            });

            if (_availableModels.Count == 0)
            {
                await Message.ErrorAsync(L["NoModelsReturned"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(_model.ModelId))
            {
                _model.ModelId = _availableModels[0].Id;
            }

            await Message.SuccessAsync(L["ModelsLoadedSuccessfully"]);
        }, LoadingKeys.LoadModels);
    }

    private async Task CloseModalAsync()
    {
        _wasOpen = false;
        await OpenChanged.InvokeAsync(false);
    }

    private bool TryApplyOpenAIApiMode()
    {
        if (string.IsNullOrWhiteSpace(_openAIApiModeText))
        {
            _model.OpenAIApiMode = null;
            return true;
        }

        if (Enum.TryParse<OpenAIApiMode>(_openAIApiModeText, out var apiMode))
        {
            _model.OpenAIApiMode = apiMode;
            return true;
        }

        _ = Message.ErrorAsync(L["InvalidOpenAIApiMode"]);
        return false;
    }

    private bool TryApplyPricing()
    {
        if (!TryParseNullableDecimal(_inputCostPer1KTokensText, out var inputCost))
        {
            _ = Message.ErrorAsync(L["InputCostPer1KTokensMustBeNonNegative"]);
            return false;
        }

        if (!TryParseNullableDecimal(_outputCostPer1KTokensText, out var outputCost))
        {
            _ = Message.ErrorAsync(L["OutputCostPer1KTokensMustBeNonNegative"]);
            return false;
        }

        _model.InputCostPer1KTokens = inputCost;
        _model.OutputCostPer1KTokens = outputCost;
        return true;
    }

    private static bool TryParseNullableDecimal(string? value, out decimal? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (decimal.TryParse(value, out var parsed) && parsed >= 0)
        {
            result = parsed;
            return true;
        }

        return false;
    }
}
