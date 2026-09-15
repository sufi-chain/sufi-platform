using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceEditModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadWorkspace = "load-workspace";
        public const string UpdateWorkspace = "update-workspace";
        public const string TestConnection = "test-connection";
        public const string LoadModels = "load-models";
        public const string ConvertWorkspace = "convert-workspace";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }
    [Parameter] public EventCallback OnConverted { get; set; }

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private WorkspaceDto? _workspace;
    private UpdateWorkspaceDto _model = new();
    private string _inputCostPer1MTokensText = string.Empty;
    private string _outputCostPer1MTokensText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private int _activeTab;
    private bool _wasOpen;
    private List<WorkspaceGuardrailFormRow> _guardrailRows = WorkspaceGuardrailForm.CreateRows();
    private List<WorkspaceGuardrailStatusDto> _guardrailStatus = new();

    private bool IsReadOnly => _workspace?.IsInherited == true;

    private string DialogTitle => IsReadOnly ? L["ViewWorkspace"] : L["EditWorkspace"];

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
                IsActive = _workspace.IsActive,
                InputCostPer1MTokens = _workspace.InputCostPer1MTokens,
                OutputCostPer1MTokens = _workspace.OutputCostPer1MTokens
            };
            _inputCostPer1MTokensText = _workspace.InputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _outputCostPer1MTokensText = _workspace.OutputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _availableModels = new List<OpenAIModelDto>
            {
                new() { Id = _workspace.Model }
            };
            _guardrailRows = WorkspaceGuardrailForm.CreateRows(_workspace.Guardrails);
            _guardrailStatus = await WorkspaceAppService.GetGuardrailStatusAsync(WorkspaceId.Value);
            _activeTab = 0;
            StateHasChanged();
        }, LoadingKeys.LoadWorkspace);
    }

    private async Task UpdateWorkspaceAsync()
    {
        if (!WorkspaceId.HasValue || IsReadOnly)
        {
            return;
        }

        _model.Provider = AIProviderType.OpenAI;

        if (!await ValidateRequiredFieldsAsync(requireName: true))
        {
            return;
        }

        if (!await TryApplyPricingAsync())
        {
            return;
        }

        if (!WorkspaceGuardrailForm.TryBuildItems(_guardrailRows, out var guardrailItems))
        {
            await Message.ErrorAsync(L["GuardrailAmountMustBeNonNegative"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.UpdateAsync(WorkspaceId.Value, _model);
            await WorkspaceAppService.UpdateGuardrailsAsync(
                WorkspaceId.Value,
                new UpdateWorkspaceGuardrailsDto { Items = guardrailItems });
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
                OpenAIApiMode = OpenAIApiMode.ChatCompletions
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
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

    private async Task ConvertToCustomAsync()
    {
        if (_workspace == null)
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(
            L["ConvertToCustomConfirmation", _workspace.Name],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.ConvertToCustomAsync(_workspace.Id);
            await CloseModal();
            await OnConverted.InvokeAsync();
        }, LoadingKeys.ConvertWorkspace);
    }

    private async Task CloseModal()
    {
        _workspace = null;
        await SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        if (!open)
        {
            _workspace = null;
        }

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
