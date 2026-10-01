using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
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

    private WorkspaceDto? _workspace;
    private readonly WorkspaceConnectionDraft _draft = new();
    private List<AiProviderProfileDto> _profiles = WorkspaceProviderCatalog.Fallback();
    private AiProviderProfileDto? SelectedProfile =>
        _profiles.FirstOrDefault(profile => profile.ProviderType == _draft.Provider);

    private Task OnProviderReset()
    {
        _availableModels = new List<OpenAIModelDto>();
        _decisionModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
        return WorkspaceId.HasValue ? LoadModelsAsync(announce: false) : Task.CompletedTask;
    }

    private List<OpenAIModelDto> _availableModels = new();
    private List<OpenAIModelDto> _decisionModels = new();
    private bool _modelsLoaded;
    private int _activeTab;
    private bool _wasOpen;
    private bool _opening;
    private List<WorkspaceGuardrailFormRow> _guardrailRows = WorkspaceGuardrailForm.CreateRows();
    private List<WorkspaceGuardrailStatusDto> _guardrailStatus = new();

    private bool IsReadOnly => _workspace?.IsInherited == true;

    private string DialogTitle => IsReadOnly ? L["ViewWorkspace"] : L["EditWorkspace"];

    protected override async Task OnParametersSetAsync()
    {
        if (Open &&
            !_opening &&
            WorkspaceId.HasValue &&
            (!_wasOpen || _workspace == null || _workspace.Id != WorkspaceId.Value))
        {
            _opening = true;
            _wasOpen = true;
            try
            {
                await LoadWorkspaceAsync();
                await LoadModelsAsync(announce: false);
            }
            finally
            {
                _opening = false;
            }
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
            try
            {
                var loadedProfiles = await WorkspaceAppService.GetProviderProfilesAsync();
                if (loadedProfiles.Count > 0)
                {
                    _profiles = loadedProfiles;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex);
            }

            _workspace = await WorkspaceAppService.GetAsync(WorkspaceId.Value);
            var loaded = WorkspaceConnectionDraft.From(_workspace);
            _draft.Name = loaded.Name;
            _draft.Provider = loaded.Provider;
            _draft.Model = loaded.Model;
            _draft.ModelDisplayName = loaded.ModelDisplayName;
            _draft.DecisionsModelId = loaded.DecisionsModelId;
            _draft.ApiKey = null;
            _draft.ApiBaseUrl = loaded.ApiBaseUrl;
            _draft.InputCostText = loaded.InputCostText;
            _draft.OutputCostText = loaded.OutputCostText;
            _availableModels = new List<OpenAIModelDto>();
            _decisionModels = new List<OpenAIModelDto>();
            _modelsLoaded = false;
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

        if (!await ValidateRequiredFieldsAsync(requireName: true))
        {
            return;
        }

        if (_workspace == null)
        {
            return;
        }

        var pricing = await TryParsePricingAsync();
        if (!pricing.Ok)
        {
            return;
        }

        var model = _draft.ToUpdate(pricing.Input, pricing.Output, _workspace.IsActive);

        if (!WorkspaceGuardrailForm.TryBuildItems(_guardrailRows, out var guardrailItems))
        {
            await Message.ErrorAsync(L["GuardrailAmountMustBeNonNegative"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.UpdateAsync(WorkspaceId.Value, model);
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

        if (!await ValidateRequiredFieldsAsync(requireName: false))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                WorkspaceId = WorkspaceId.Value,
                Model = _draft.Model,
                ApiKey = _draft.ApiKey,
                ApiBaseUrl = _draft.ApiBaseUrl,
                Provider = _draft.Provider,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
    }

    private Task LoadModelsAsync() => LoadModelsAsync(announce: true);

    private async Task LoadModelsAsync(bool announce)
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
                {
                    WorkspaceId = WorkspaceId.Value,
                    ApiKey = _draft.ApiKey,
                    ApiBaseUrl = _draft.ApiBaseUrl,
                    Provider = _draft.Provider,
                    CapabilityType = AICapabilityType.ChatCompletion
                });
                _decisionModels = SelectedProfile?.SupportsDecisions == true
                    ? await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
                    {
                        WorkspaceId = WorkspaceId.Value,
                        ApiKey = _draft.ApiKey,
                        ApiBaseUrl = _draft.ApiBaseUrl,
                        Provider = _draft.Provider,
                        CapabilityType = AICapabilityType.Decisions
                    })
                    : new List<OpenAIModelDto>();
            }
            catch (Exception ex)
            {
                await Notify.ErrorAsync(ex.Message);
                return;
            }

            _modelsLoaded = _availableModels.Count > 0;

            if (_availableModels.Count == 0)
            {
                await Notify.WarnAsync(L["NoModelsReturned"]);
                return;
            }

            if (announce)
            {
                await Notify.SuccessAsync(L["ModelsLoadedSuccessfully"]);
            }
        }, LoadingKeys.LoadModels);
    }

    private async Task<bool> ValidateRequiredFieldsAsync(bool requireName)
    {
        if (requireName && string.IsNullOrWhiteSpace(_draft.Name))
        {
            await Message.ErrorAsync(L["WorkspaceNameRequired"]);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_draft.Model))
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

    private async Task<(bool Ok, decimal? Input, decimal? Output)> TryParsePricingAsync()
    {
        if (!TryParseNullableDecimal(_draft.InputCostText, out var inputCost))
        {
            await Message.ErrorAsync(L["InputCostPer1MTokensMustBeNonNegative"]);
            return (false, null, null);
        }

        if (!TryParseNullableDecimal(_draft.OutputCostText, out var outputCost))
        {
            await Message.ErrorAsync(L["OutputCostPer1MTokensMustBeNonNegative"]);
            return (false, null, null);
        }

        return (true, inputCost, outputCost);
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
