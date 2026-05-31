using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Components;

public partial class WorkspaceEditModal : AIManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspace = "load-workspace";
        public const string UpdateWorkspace = "update-workspace";
        public const string TestConnection = "test-connection";
        public const string LoadModels = "load-models";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private WorkspaceDto? _workspace;
    private UpdateWorkspaceDto _model = new();
    private string _temperatureText = "0.7";
    private string _maxTokensText = "2000";
    private string _inputCostPer1KTokensText = string.Empty;
    private string _outputCostPer1KTokensText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private int _activeTab;
    private bool _wasOpen;

    protected override async Task OnParametersSetAsync()
    {
        if (Open && WorkspaceId.HasValue && (!_wasOpen || _workspace == null || _workspace.Id != WorkspaceId.Value))
        {
            await LoadWorkspaceAsync();
        }

        _wasOpen = Open;
    }

    private async Task LoadWorkspaceAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _workspace = await WorkspaceAppService.GetAsync(WorkspaceId.Value);
            _model = new UpdateWorkspaceDto
            {
                Name = _workspace.Name,
                Provider = AIProviderType.OpenAI,
                Model = _workspace.Model,
                ApiBaseUrl = _workspace.ApiBaseUrl,
                SystemPrompt = _workspace.SystemPrompt,
                Temperature = _workspace.Temperature,
                MaxTokens = _workspace.MaxTokens,
                IsActive = _workspace.IsActive,
                OpenAIApiMode = _workspace.OpenAIApiMode,
                InputCostPer1KTokens = _workspace.InputCostPer1KTokens,
                OutputCostPer1KTokens = _workspace.OutputCostPer1KTokens
            };
            _temperatureText = _workspace.Temperature.ToString("0.##", CultureInfo.InvariantCulture);
            _maxTokensText = _workspace.MaxTokens.ToString(CultureInfo.InvariantCulture);
            _inputCostPer1KTokensText = _workspace.InputCostPer1KTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _outputCostPer1KTokensText = _workspace.OutputCostPer1KTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _availableModels = new List<OpenAIModelDto>
            {
                new() { Id = _workspace.Model }
            };
            _activeTab = 0;
            StateHasChanged();
        }, LoadingKeys.LoadWorkspace);
    }

    private async Task UpdateWorkspaceAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        _model.Provider = AIProviderType.OpenAI;

        if (!await ValidateRequiredFieldsAsync(requireName: true))
        {
            return;
        }

        if (!await TryApplyGenerationSettingsAsync())
        {
            return;
        }

        if (!await TryApplyPricingAsync())
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.UpdateAsync(WorkspaceId.Value, _model);
            await CloseModal();
            await OnUpdated.InvokeAsync();
        }, LoadingKeys.UpdateWorkspace);
    }

    private async Task TestConnectionAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        _model.Provider = AIProviderType.OpenAI;

        if (!await ValidateRequiredFieldsAsync(requireName: false))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                WorkspaceId = WorkspaceId.Value,
                Model = _model.Model,
                ApiKey = _model.ApiKey,
                ApiBaseUrl = _model.ApiBaseUrl,
                OpenAIApiMode = _model.OpenAIApiMode
            });
            await Message.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
    }

    private async Task LoadModelsAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
            {
                WorkspaceId = WorkspaceId.Value,
                ApiKey = _model.ApiKey,
                ApiBaseUrl = _model.ApiBaseUrl
            });

            if (_availableModels.Count == 0)
            {
                await Message.ErrorAsync(L["NoModelsReturned"]);
                return;
            }

            if (string.IsNullOrWhiteSpace(_model.Model))
            {
                _model.Model = _availableModels[0].Id;
            }

            await Message.SuccessAsync(L["ModelsLoadedSuccessfully"]);
        }, LoadingKeys.LoadModels);
    }

    private async Task<bool> ValidateRequiredFieldsAsync(bool requireName)
    {
        if (requireName && string.IsNullOrWhiteSpace(_model.Name))
        {
            await Message.ErrorAsync(L["WorkspaceNameRequired"]);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_model.Model))
        {
            await Message.ErrorAsync(L["ModelIdRequired"]);
            return false;
        }

        return true;
    }

    private async Task<bool> TryApplyGenerationSettingsAsync()
    {
        if (!TryParseFloat(_temperatureText, out var temp))
        {
            await Message.ErrorAsync(L["TemperatureMustBeNumber"]);
            return false;
        }

        if (!int.TryParse(_maxTokensText, out var tokens))
        {
            await Message.ErrorAsync(L["MaxTokensMustBeNumber"]);
            return false;
        }

        _model.Temperature = temp;
        _model.MaxTokens = tokens;
        return true;
    }

    private async Task CloseModal()
    {
        _workspace = null;
        Open = false;
        _wasOpen = false;
        await OpenChanged.InvokeAsync(Open);
    }

    private async Task<bool> TryApplyPricingAsync()
    {
        if (!TryParseNullableDecimal(_inputCostPer1KTokensText, out var inputCost))
        {
            await Message.ErrorAsync(L["InputCostPer1KTokensMustBeNonNegative"]);
            return false;
        }

        if (!TryParseNullableDecimal(_outputCostPer1KTokensText, out var outputCost))
        {
            await Message.ErrorAsync(L["OutputCostPer1KTokensMustBeNonNegative"]);
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

        if (TryParseDecimal(value, out var parsed) && parsed >= 0)
        {
            result = parsed;
            return true;
        }

        return false;
    }

    private static bool TryParseFloat(string? value, out float result)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ||
               float.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
    }
}
