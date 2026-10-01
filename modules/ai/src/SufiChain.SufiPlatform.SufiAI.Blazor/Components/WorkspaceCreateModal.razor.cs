using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
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

    private readonly WorkspaceConnectionDraft _draft = WorkspaceConnectionDraft.ForCreate();
    private List<OpenAIModelDto> _availableModels = new();
    private List<OpenAIModelDto> _decisionModels = new();
    private bool _modelsLoaded;
    private int _activeTab;
    private bool _wasOpen;
    private List<WorkspaceGuardrailFormRow> _guardrailRows = WorkspaceGuardrailForm.CreateRows();
    private List<AiProviderProfileDto> _profiles = WorkspaceProviderCatalog.Fallback();
    private AiProviderProfileDto? SelectedProfile =>
        _profiles.FirstOrDefault(profile => profile.ProviderType == _draft.Provider);

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            ResetForm();
            _ = LoadProfilesAsync();
        }

        _wasOpen = Open;
    }

    private Task LoadProfilesAsync()
    {
        return ExecuteWithLoadingAsync(async () =>
        {
            var loadedProfiles = await WorkspaceAppService.GetProviderProfilesAsync();
            if (loadedProfiles.Count > 0)
            {
                _profiles = loadedProfiles;
            }
            if (string.IsNullOrWhiteSpace(_draft.ApiBaseUrl) &&
                SelectedProfile is { RequiresExplicitBaseUrl: false, DefaultBaseUrl: { Length: > 0 } } profile)
            {
                _draft.ApiBaseUrl = profile.DefaultBaseUrl;
            }
        });
    }

    private Task OnProviderReset()
    {
        _availableModels = new List<OpenAIModelDto>();
        _decisionModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
        return Task.CompletedTask;
    }

    private void ResetForm()
    {
        var fresh = WorkspaceConnectionDraft.ForCreate();
        _draft.Name = fresh.Name;
        _draft.Provider = fresh.Provider;
        _draft.Model = fresh.Model;
        _draft.ModelDisplayName = fresh.ModelDisplayName;
        _draft.DecisionsModelId = fresh.DecisionsModelId;
        _draft.ApiKey = fresh.ApiKey;
        _draft.ApiBaseUrl = fresh.ApiBaseUrl;
        _draft.InputCostText = fresh.InputCostText;
        _draft.OutputCostText = fresh.OutputCostText;
        _availableModels = new List<OpenAIModelDto>();
        _decisionModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
        _guardrailRows = WorkspaceGuardrailForm.CreateRows();
        _activeTab = 0;
    }

    private async Task CreateWorkspaceAsync()
    {

        if (!await ValidateRequiredFieldsAsync(requireName: true, requireApiKey: false))
        {
            return;
        }

        var pricing = await TryParsePricingAsync();
        if (!pricing.Ok)
        {
            return;
        }

        if (!WorkspaceGuardrailForm.TryBuildItems(_guardrailRows, out var guardrails))
        {
            await Message.ErrorAsync(L["GuardrailAmountMustBeNonNegative"]);
            return;
        }

        var model = _draft.ToCreate(pricing.Input, pricing.Output);
        model.Guardrails = guardrails;

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.CreateAsync(model);
            await CloseModal();
            await OnCreated.InvokeAsync();
        }, LoadingKeys.CreateWorkspace);
    }

    private async Task TestConnectionAsync()
    {
        if (!await ValidateRequiredFieldsAsync(requireName: false, requireApiKey: true))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                Model = _draft.Model,
                ApiKey = _draft.ApiKey,
                ApiBaseUrl = _draft.ApiBaseUrl,
                Provider = _draft.Provider,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
    }

    private async Task LoadModelsAsync()
    {
        if (string.IsNullOrWhiteSpace(_draft.ApiKey))
        {
            await Notify.ErrorAsync(L["ApiKeyRequiredForModelList"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
                {
                    ApiKey = _draft.ApiKey,
                    ApiBaseUrl = _draft.ApiBaseUrl,
                    Provider = _draft.Provider,
                    CapabilityType = AICapabilityType.ChatCompletion
                });
                _decisionModels = SelectedProfile?.SupportsDecisions == true
                    ? await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
                    {
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

            await Notify.SuccessAsync(L["ModelsLoadedSuccessfully"]);
        }, LoadingKeys.LoadModels);
    }

    private async Task<bool> ValidateRequiredFieldsAsync(bool requireName, bool requireApiKey)
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

        if (requireApiKey && string.IsNullOrWhiteSpace(_draft.ApiKey))
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
