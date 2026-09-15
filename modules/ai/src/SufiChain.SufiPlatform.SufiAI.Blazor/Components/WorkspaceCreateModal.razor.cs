using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceCreateModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string CreateWorkspace = "create-workspace";
        public const string TestConnection = "test-connection";
        public const string LoadModels = "load-models";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnCreated { get; set; }

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private CreateWorkspaceDto _model = new();
    private string _inputCostPer1MTokensText = string.Empty;
    private string _outputCostPer1MTokensText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private int _activeTab;
    private bool _wasOpen;
    private List<WorkspaceGuardrailFormRow> _guardrailRows = WorkspaceGuardrailForm.CreateRows();

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            ResetForm();
        }

        _wasOpen = Open;
    }

    private void ResetForm()
    {
        _model = new CreateWorkspaceDto
        {
            Provider = AIProviderType.OpenAI
        };
        _inputCostPer1MTokensText = string.Empty;
        _outputCostPer1MTokensText = string.Empty;
        _availableModels = new List<OpenAIModelDto>();
        _guardrailRows = WorkspaceGuardrailForm.CreateRows();
        _activeTab = 0;
    }

    private async Task CreateWorkspaceAsync()
    {
        _model.Provider = AIProviderType.OpenAI;

        if (!await ValidateRequiredFieldsAsync(requireName: true, requireApiKey: false))
        {
            return;
        }

        if (!await TryApplyPricingAsync())
        {
            return;
        }

        if (!WorkspaceGuardrailForm.TryBuildItems(_guardrailRows, out var guardrails))
        {
            await Message.ErrorAsync(L["GuardrailAmountMustBeNonNegative"]);
            return;
        }

        _model.Guardrails = guardrails;

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.CreateAsync(_model);
            await CloseModal();
            await OnCreated.InvokeAsync();
        }, LoadingKeys.CreateWorkspace);
    }

    private async Task TestConnectionAsync()
    {
        _model.Provider = AIProviderType.OpenAI;

        if (!await ValidateRequiredFieldsAsync(requireName: false, requireApiKey: true))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                Model = _model.Model,
                ApiKey = _model.ApiKey,
                ApiBaseUrl = _model.ApiBaseUrl,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
    }

    private async Task LoadModelsAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.ApiKey))
        {
            await Message.ErrorAsync(L["ApiKeyRequiredForModelList"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
            {
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

    private async Task<bool> ValidateRequiredFieldsAsync(bool requireName, bool requireApiKey)
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

        if (requireApiKey && string.IsNullOrWhiteSpace(_model.ApiKey))
        {
            await Message.ErrorAsync(L["ApiKeyRequiredForConnectionTest"]);
            return false;
        }

        return true;
    }

    private async Task CloseModal()
    {
        await SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }

    private async Task<bool> TryApplyPricingAsync()
    {
        if (!TryParseNullableDecimal(_inputCostPer1MTokensText, out var inputCost))
        {
            await Message.ErrorAsync(L["InputCostPer1MTokensMustBeNonNegative"]);
            return false;
        }

        if (!TryParseNullableDecimal(_outputCostPer1MTokensText, out var outputCost))
        {
            await Message.ErrorAsync(L["OutputCostPer1MTokensMustBeNonNegative"]);
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

        if (TryParseDecimal(value, out var parsed) && parsed >= 0)
        {
            result = parsed;
            return true;
        }

        return false;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ||
               decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
    }
}
