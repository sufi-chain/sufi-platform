using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class ModelConfigurationModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string SaveConfiguration = "save-configuration";
        public const string LoadModels = "load-models";
        public const string TestConnection = "test-connection";
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
    private bool _hasStoredApiKey;
    private CreateAIModelConfigurationDto _model = new();
    private string _priorityText = "0";
    private string _openAIApiModeText = string.Empty;
    private string _inputCostPer1MTokensText = string.Empty;
    private string _outputCostPer1MTokensText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private int _activeTab;
    private bool _wasOpen;

    private bool ShowsOpenAIApiMode =>
        _model.CapabilityType is AICapabilityType.ChatCompletion or AICapabilityType.VisionAnalysis;

    private bool ShowsEndpointOverrideHint =>
        !string.IsNullOrWhiteSpace(_model.ApiEndpoint);

    private string ApiKeyPlaceholder =>
        _isEditMode && _hasStoredApiKey
            ? L["LeaveEmptyToKeepCurrent"]
            : L["ApiKeyPlaceholder"];

    private string ApiKeyHelperText =>
        _isEditMode && _hasStoredApiKey
            ? L["LeaveEmptyToKeepCurrent"]
            : L["ApiKeyHelperText"];

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
            _hasStoredApiKey = Configuration.HasApiKey;
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = Configuration.WorkspaceId,
                CapabilityType = Configuration.CapabilityType,
                ModelId = Configuration.ModelId,
                ApiEndpoint = Configuration.ApiEndpoint,
                OpenAIApiMode = Configuration.OpenAIApiMode,
                InputCostPer1MTokens = Configuration.InputCostPer1MTokens,
                OutputCostPer1MTokens = Configuration.OutputCostPer1MTokens,
                Priority = Configuration.Priority
            };
            _priorityText = Configuration.Priority.ToString();
            _openAIApiModeText = Configuration.OpenAIApiMode?.ToString() ?? string.Empty;
            _inputCostPer1MTokensText = Configuration.InputCostPer1MTokens?.ToString() ?? string.Empty;
            _outputCostPer1MTokensText = Configuration.OutputCostPer1MTokens?.ToString() ?? string.Empty;
            _availableModels = new List<OpenAIModelDto>
            {
                new() { Id = Configuration.ModelId }
            };
            _activeTab = 0;
        }
        else
        {
            _hasStoredApiKey = false;
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = WorkspaceId ?? Guid.Empty,
                CapabilityType = AICapabilityType.ChatCompletion,
                Priority = 0
            };
            _priorityText = "0";
            _openAIApiModeText = string.Empty;
            _inputCostPer1MTokensText = string.Empty;
            _outputCostPer1MTokensText = string.Empty;
            _availableModels = new List<OpenAIModelDto>();
            _activeTab = 0;
        }
    }

    private void OnCapabilityChanged(AICapabilityType value)
    {
        _model.CapabilityType = value;
        if (!ShowsOpenAIApiMode)
        {
            _openAIApiModeText = string.Empty;
            _model.OpenAIApiMode = null;
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

        if (int.TryParse(_priorityText, out var priority))
        {
            _model.Priority = priority;
        }
        else
        {
            await Message.ErrorAsync(L["PriorityMustBeNumber"]);
            return;
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
                    OpenAIApiMode = _model.OpenAIApiMode,
                    InputCostPer1MTokens = _model.InputCostPer1MTokens,
                    OutputCostPer1MTokens = _model.OutputCostPer1MTokens,
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

    private async Task TestConnectionAsync()
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

        if (!TryApplyOpenAIApiMode())
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                WorkspaceId = WorkspaceId.Value,
                ModelConfigurationId = Configuration?.Id,
                CapabilityType = _model.CapabilityType,
                Model = _model.ModelId,
                ApiKey = _model.ApiKey,
                ApiBaseUrl = _model.ApiEndpoint,
                OpenAIApiMode = _model.OpenAIApiMode ?? OpenAIApiMode.ChatCompletions
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
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
                ModelConfigurationId = Configuration?.Id,
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
        await SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }

    private bool TryApplyOpenAIApiMode()
    {
        if (!ShowsOpenAIApiMode || string.IsNullOrWhiteSpace(_openAIApiModeText))
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
        if (!TryParseNullableDecimal(_inputCostPer1MTokensText, out var inputCost))
        {
            _ = Message.ErrorAsync(L["InputCostPer1MTokensMustBeNonNegative"]);
            return false;
        }

        if (!TryParseNullableDecimal(_outputCostPer1MTokensText, out var outputCost))
        {
            _ = Message.ErrorAsync(L["OutputCostPer1MTokensMustBeNonNegative"]);
            return false;
        }

        _model.InputCostPer1MTokens = inputCost;
        _model.OutputCostPer1MTokens = outputCost;
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
