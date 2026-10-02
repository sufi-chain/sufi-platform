using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor.Components;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.SufiAI.Blazor;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WorkspaceEditor : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadPage = "load-page";
        public const string CreateWorkspace = "create-workspace";
        public const string UpdateWorkspace = "update-workspace";
        public const string TestConnection = "test-connection";
        public const string LoadModels = "load-models";
        public const string ConvertWorkspace = "convert-workspace";
    }

    [Parameter]
    public Guid WorkspaceId { get; set; }

    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    private readonly WorkspaceConnectionDraft _draft = new();
    private WorkspaceDto? _workspace;
    private List<AiProviderProfileDto> _profiles = WorkspaceProviderCatalog.Fallback();
    private List<OpenAIModelDto> _availableModels = new();
    private List<OpenAIModelDto> _decisionModels = new();
    private List<WorkspaceGuardrailFormRow> _guardrailRows = WorkspaceGuardrailForm.CreateRows();
    private List<WorkspaceGuardrailStatusDto> _guardrailStatus = new();
    private bool _modelsLoaded;
    private int _activeTab;
    private bool _ready;
    private bool _notFound;
    private bool _hasLoaded;
    private bool _loadedCreateRoute;
    private Guid _loadedWorkspaceId;
    private int _loadVersion;
    private string? _appliedTitle;

    private AiProviderProfileDto? SelectedProfile =>
        _profiles.FirstOrDefault(profile => profile.ProviderType == _draft.Provider);

    private bool IsCreateRoute
    {
        get
        {
            var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
            var path = relative.Split('?', '#')[0].TrimEnd('/');
            return path.EndsWith("/workspaces/new", StringComparison.OrdinalIgnoreCase);
        }
    }

    private bool IsEdit => !IsCreateRoute;

    private bool IsReadOnly => _workspace?.IsInherited == true;

    private bool IsSaving =>
        IsOperationLoading(IsEdit ? LoadingKeys.UpdateWorkspace : LoadingKeys.CreateWorkspace);

    private string SavePolicy =>
        IsEdit ? AIPermissions.Workspaces.Edit : AIPermissions.Workspaces.Create;

    protected override async Task OnParametersSetAsync()
    {
        ApplyTitle();
        if (!IsInteractive || SameLoad())
        {
            return;
        }

        await LoadPageAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await LoadPageAsync();
        }
    }

    private bool SameLoad() =>
        _hasLoaded && IsCreateRoute == _loadedCreateRoute && WorkspaceId == _loadedWorkspaceId;

    private void ApplyTitle()
    {
        var title = IsCreateRoute
            ? L["NewWorkspace"].Value
            : IsReadOnly
                ? L["ViewWorkspace"].Value
                : L["EditWorkspace"].Value;
        if (string.Equals(_appliedTitle, title, StringComparison.Ordinal))
        {
            return;
        }

        _appliedTitle = title;
        PageLayout.Title = title;
    }

    private async Task LoadPageAsync()
    {
        if (SameLoad())
        {
            return;
        }

        var version = ++_loadVersion;
        var createRoute = IsCreateRoute;
        var workspaceId = WorkspaceId;
        _ready = false;
        _notFound = false;

        await ExecuteWithLoadingAsync(async () =>
        {
            if (version != _loadVersion)
            {
                return;
            }

            await LoadProfilesAsync();
            if (version != _loadVersion)
            {
                return;
            }

            if (createRoute)
            {
                ResetCreateForm();
                ApplyCreateDefaultBaseUrl();
                _ready = true;
                return;
            }

            if (workspaceId == Guid.Empty)
            {
                _notFound = true;
                return;
            }

            try
            {
                _workspace = await WorkspaceAppService.GetAsync(workspaceId);
            }
            catch (Exception ex) when (IsEntityNotFound(ex))
            {
                _workspace = null;
                _notFound = true;
                return;
            }

            if (version != _loadVersion || _workspace == null)
            {
                return;
            }

            ApplyDraft(WorkspaceConnectionDraft.From(_workspace));
            _draft.ApiKey = null;
            _availableModels = new List<OpenAIModelDto>();
            _decisionModels = new List<OpenAIModelDto>();
            _modelsLoaded = false;
            _guardrailRows = WorkspaceGuardrailForm.CreateRows(_workspace.Guardrails);
            _guardrailStatus = await WorkspaceAppService.GetGuardrailStatusAsync(workspaceId);
            _activeTab = 0;
            _ready = true;
        }, LoadingKeys.LoadPage);

        if (version != _loadVersion)
        {
            return;
        }

        _loadedCreateRoute = createRoute;
        _loadedWorkspaceId = workspaceId;
        _hasLoaded = true;
        ApplyTitle();

        if (_ready && !createRoute)
        {
            await LoadModelsAsync(announce: false);
        }
    }

    private async Task LoadProfilesAsync()
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
    }

    private void ApplyCreateDefaultBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(_draft.ApiBaseUrl) &&
            SelectedProfile is { RequiresExplicitBaseUrl: false, DefaultBaseUrl: { Length: > 0 } } profile)
        {
            _draft.ApiBaseUrl = profile.DefaultBaseUrl;
        }
    }

    private void ResetCreateForm()
    {
        _workspace = null;
        ApplyDraft(WorkspaceConnectionDraft.ForCreate());
        _availableModels = new List<OpenAIModelDto>();
        _decisionModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
        _guardrailRows = WorkspaceGuardrailForm.CreateRows();
        _guardrailStatus = new List<WorkspaceGuardrailStatusDto>();
        _activeTab = 0;
    }

    private void ApplyDraft(WorkspaceConnectionDraft loaded)
    {
        _draft.Name = loaded.Name;
        _draft.Provider = loaded.Provider;
        _draft.Model = loaded.Model;
        _draft.ModelDisplayName = loaded.ModelDisplayName;
        _draft.DecisionsModelId = loaded.DecisionsModelId;
        _draft.ApiKey = loaded.ApiKey;
        _draft.ApiBaseUrl = loaded.ApiBaseUrl;
        _draft.InputCostText = loaded.InputCostText;
        _draft.OutputCostText = loaded.OutputCostText;
    }

    private Task OnProviderReset()
    {
        _availableModels = new List<OpenAIModelDto>();
        _decisionModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
        return IsEdit && WorkspaceId != Guid.Empty
            ? LoadModelsAsync(announce: false)
            : Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (IsEdit)
        {
            await UpdateWorkspaceAsync();
            return;
        }

        await CreateWorkspaceAsync();
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
            await Message.SuccessAsync(L["WorkspaceCreatedSuccessfully"]);
            Cancel();
        }, LoadingKeys.CreateWorkspace);
    }

    private async Task UpdateWorkspaceAsync()
    {
        if (WorkspaceId == Guid.Empty || IsReadOnly || _workspace == null)
        {
            return;
        }

        if (!await ValidateRequiredFieldsAsync(requireName: true, requireApiKey: false))
        {
            return;
        }

        var pricing = await TryParsePricingAsync();
        if (!pricing.Ok)
        {
            return;
        }

        if (!WorkspaceGuardrailForm.TryBuildItems(_guardrailRows, out var guardrailItems))
        {
            await Message.ErrorAsync(L["GuardrailAmountMustBeNonNegative"]);
            return;
        }

        var model = _draft.ToUpdate(pricing.Input, pricing.Output, _workspace.IsActive);
        var workspaceId = WorkspaceId;

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.UpdateAsync(workspaceId, model);
            await WorkspaceAppService.UpdateGuardrailsAsync(
                workspaceId,
                new UpdateWorkspaceGuardrailsDto { Items = guardrailItems });
            await Message.SuccessAsync(L["WorkspaceUpdatedSuccessfully"]);
            Cancel();
        }, LoadingKeys.UpdateWorkspace);
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

        var workspaceId = _workspace.Id;
        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.ConvertToCustomAsync(workspaceId);
            await Message.SuccessAsync(L["WorkspaceConvertedSuccessfully"]);
            Cancel();
        }, LoadingKeys.ConvertWorkspace);
    }

    private async Task TestConnectionAsync()
    {
        if (IsReadOnly)
        {
            return;
        }

        if (!await ValidateRequiredFieldsAsync(requireName: false, requireApiKey: !IsEdit))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.TestConnectionAsync(new TestWorkspaceConnectionInput
            {
                WorkspaceId = IsEdit ? WorkspaceId : null,
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
        if (!IsEdit && string.IsNullOrWhiteSpace(_draft.ApiKey))
        {
            await Notify.ErrorAsync(L["ApiKeyRequiredForModelList"]);
            return;
        }

        if (IsEdit && WorkspaceId == Guid.Empty)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(
                    CreateModelsInput(AICapabilityType.ChatCompletion));
                _decisionModels = SelectedProfile?.SupportsDecisions == true
                    ? await WorkspaceAppService.GetAvailableModelsAsync(
                        CreateModelsInput(AICapabilityType.Decisions))
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

    private GetOpenAIModelsInput CreateModelsInput(AICapabilityType capability)
    {
        return new GetOpenAIModelsInput
        {
            WorkspaceId = IsEdit ? WorkspaceId : null,
            ApiKey = _draft.ApiKey,
            ApiBaseUrl = _draft.ApiBaseUrl,
            Provider = _draft.Provider,
            CapabilityType = capability
        };
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

    private void Cancel()
    {
        Navigation.NavigateTo("/panel/admin/ai/workspaces");
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

    private static bool IsEntityNotFound(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().Name, "EntityNotFoundException", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
