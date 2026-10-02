using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Blazor.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.SufiAI.Blazor;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class ModelConfigurationEditor : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadPage = "load-page";
        public const string SaveConfiguration = "save-configuration";
        public const string LoadModels = "load-models";
        public const string TestConnection = "test-connection";
    }

    [Parameter]
    public Guid WorkspaceId { get; set; }

    [Parameter]
    public Guid? ConfigurationId { get; set; }

    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private bool _isEditMode;
    private bool _hasStoredApiKey;
    private bool _useWorkspaceConnection = true;
    private bool _ready;
    private bool _notFound;
    private bool _readOnly;
    private bool _hasLoaded;
    private string? _appliedTitle;
    private Guid _loadedWorkspaceId;
    private Guid? _loadedConfigurationId;
    private int _formKey;
    private bool _showManualModelId;
    private CreateAIModelConfigurationDto _model = new();
    private AIModelConfigurationDto? _editing;
    private WorkspaceDto? _workspace;
    private bool _inputPriceSpecified;
    private decimal _inputPriceValue;
    private bool _outputPriceSpecified;
    private decimal _outputPriceValue;
    private bool _showOutputPrice = true;
    private List<OpenAIModelDto> _availableModels = new();
    private bool _modelsLoaded;
    private List<AiProviderProfileDto> _profiles = new();

    private bool IsCreateRoute
    {
        get
        {
            var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
            var path = relative.Split('?', '#')[0].TrimEnd('/');
            return path.EndsWith("/model-configurations/new", StringComparison.OrdinalIgnoreCase);
        }
    }

    private Guid? ResolvedConfigurationId => IsCreateRoute ? null : ConfigurationId;

    private bool IsEdit => ResolvedConfigurationId.HasValue;

    private string? ModelSelectValue => string.IsNullOrWhiteSpace(_model.ModelId) ? null : _model.ModelId;

    private string ManualModelId
    {
        get => _model.ModelId;
        set => _model.ModelId = value;
    }

    private bool ShowManualModelIdField =>
        _showManualModelId || (_modelsLoaded && _availableModels.Count == 0);

    private string ModelListEmptyText =>
        !_modelsLoaded
            ? L["ModelsNotLoaded"].Value
            : _availableModels.Count == 0
                ? L["NoModelsMatchCapability"].Value
                : L["NoMatchingModels"].Value;

    private string? ConnectionApiKey => _useWorkspaceConnection ? null : _model.ApiKey;

    private string? ConnectionBaseUrl => _useWorkspaceConnection ? null : _model.ApiEndpoint;

    private Guid? ConnectionConfigurationId => _useWorkspaceConnection ? null : _editing?.Id;

    private IReadOnlyList<OpenAIModelDto> SelectableModels =>
        WorkspaceConnectionDraft.WithCurrent(_availableModels, _model.ModelId, _model.DisplayName);

    private AIProviderType WorkspaceProvider => _workspace?.Provider ?? AIProviderType.OpenAI;

    private string ProviderDisplayName =>
        _profiles.FirstOrDefault(profile => profile.ProviderType == WorkspaceProvider)?.DisplayName
        ?? WorkspaceProvider.ToString();

    private bool ShowsUserSelectable =>
        _model.CapabilityType == AICapabilityType.ChatCompletion;

    private bool ShowsEmbeddingDimensions =>
        _model.CapabilityType == AICapabilityType.Embeddings;

    private bool ModelChanged =>
        _isEditMode &&
        _editing != null &&
        !string.Equals(_editing.ModelId?.Trim(), _model.ModelId?.Trim(), StringComparison.OrdinalIgnoreCase);

    private string CapabilityTitle =>
        L[_model.CapabilityType switch
        {
            AICapabilityType.ChatCompletion => "ChatCompletion",
            AICapabilityType.AudioTranscription => "AudioTranscription",
            AICapabilityType.TextToSpeech => "TextToSpeech",
            AICapabilityType.VisionAnalysis => "VisionAnalysis",
            AICapabilityType.Embeddings => "Embeddings",
            AICapabilityType.ImageGeneration => "ImageGeneration",
            AICapabilityType.WebSearch => "WebSearch",
            AICapabilityType.WebFetch => "WebFetch",
            _ => "Capability"
        }];

    private string CapabilityDescription =>
        L[_model.CapabilityType switch
        {
            AICapabilityType.Embeddings => "EmbeddingCapabilityDescription",
            AICapabilityType.WebSearch => "WebSearchCapabilityDescription",
            AICapabilityType.WebFetch => "WebFetchCapabilityDescription",
            AICapabilityType.AudioTranscription => "AudioCapabilityDescription",
            AICapabilityType.TextToSpeech => "TextToSpeechCapabilityDescription",
            AICapabilityType.VisionAnalysis => "VisionCapabilityDescription",
            AICapabilityType.ImageGeneration => "ImageGenerationCapabilityDescription",
            _ => "ChatCapabilityDescription"
        }];

    private OpenAIModelDto? SelectedCatalogModel =>
        _availableModels.FirstOrDefault(model => string.Equals(model.Id, _model.ModelId, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<string> EffortOptions
    {
        get
        {
            var options = new List<string>();
            AddEfforts(options, SelectedCatalogModel?.ReasoningEfforts);
            AddEfforts(options, SplitEfforts(_model.ReasoningEfforts));
            return options;
        }
    }

    private IReadOnlyList<string> AllowedEfforts => SplitEfforts(_model.ReasoningEfforts);

    private bool AcceptsImageInputValue
    {
        get => _model.AcceptsImageInput == true;
        set
        {
            _model.AcceptsImageInput = value;
            _model.CapabilitySource = ModelCapabilitySource.Manual;
        }
    }

    private bool AcceptsFileInputValue
    {
        get => _model.AcceptsFileInput == true;
        set
        {
            _model.AcceptsFileInput = value;
            _model.CapabilitySource = ModelCapabilitySource.Manual;
        }
    }

    private bool SupportsReasoningValue
    {
        get => _model.SupportsReasoning == true;
        set
        {
            _model.SupportsReasoning = value;
            _model.CapabilitySource = ModelCapabilitySource.Manual;
        }
    }

    private decimal InputPriceValue
    {
        get => _inputPriceValue;
        set
        {
            _inputPriceValue = value < 0m ? 0m : value;
            _inputPriceSpecified = true;
        }
    }

    private decimal OutputPriceValue
    {
        get => _outputPriceValue;
        set
        {
            _outputPriceValue = value < 0m ? 0m : value;
            _outputPriceSpecified = true;
        }
    }

    private int MaxContextTokensValue
    {
        get => _model.MaxContextTokens;
        set => _model.MaxContextTokens = value < 1
            ? AIModelConfigurationConsts.DefaultMaxContextTokens
            : value;
    }

    private int DimensionsValue
    {
        get => _model.Dimensions ?? 0;
        set => _model.Dimensions = value > 0 ? value : null;
    }

    private string EmbeddingDimensionsPlaceholder =>
        EmbeddingModelDefaults.GetDimensions(_model.ModelId).ToString();

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
        _hasLoaded && WorkspaceId == _loadedWorkspaceId && ResolvedConfigurationId == _loadedConfigurationId;

    private void ApplyTitle()
    {
        var title = IsEdit ? L["EditModelConfiguration"].Value : L["NewModelConfiguration"].Value;
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

        var workspaceId = WorkspaceId;
        var configurationId = ResolvedConfigurationId;
        _ready = false;
        _notFound = false;

        await ExecuteWithLoadingAsync(async () =>
        {
            _workspace = await WorkspaceAppService.GetAsync(workspaceId);
            _readOnly = _workspace.IsInherited;
            _profiles = await WorkspaceAppService.GetProviderProfilesAsync();

            if (configurationId.HasValue)
            {
                var configurations = await AIAppService.GetModelConfigurationsAsync(workspaceId);
                _editing = configurations.FirstOrDefault(item =>
                    item.Id == configurationId.Value && item.WorkspaceId == workspaceId);
                if (_editing == null)
                {
                    _notFound = true;
                    _isEditMode = false;
                    return;
                }
            }
            else
            {
                _editing = null;
            }

            _isEditMode = _editing != null;
            ResetForm();
            _ready = true;
        }, LoadingKeys.LoadPage);

        _loadedWorkspaceId = workspaceId;
        _loadedConfigurationId = configurationId;
        _hasLoaded = true;

        if (_ready)
        {
            await LoadModelsAsync(announce: false);
        }
    }

    private static bool UsesWorkspaceConnection(AIModelConfigurationDto configuration) =>
        string.IsNullOrWhiteSpace(configuration.ApiEndpoint) && !configuration.HasApiKey;

    private void ResetForm()
    {
        _formKey++;
        if (_isEditMode && _editing != null)
        {
            _hasStoredApiKey = _editing.HasApiKey;
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = _editing.WorkspaceId,
                CapabilityType = _editing.CapabilityType,
                ModelId = _editing.ModelId,
                DisplayName = _editing.DisplayName,
                Description = _editing.Description,
                IsUserSelectable = _editing.IsUserSelectable,
                ApiEndpoint = _editing.ApiEndpoint,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                MaxContextTokens = _editing.MaxContextTokens > 0
                    ? _editing.MaxContextTokens
                    : AIModelConfigurationConsts.DefaultMaxContextTokens,
                InputPrice = _editing.InputPrice,
                InputPriceUnit = _editing.InputPriceUnit,
                OutputPrice = _editing.OutputPrice,
                OutputPriceUnit = _editing.OutputPriceUnit,
                Priority = _editing.Priority,
                Dimensions = _editing.Dimensions,
                AcceptsImageInput = _editing.AcceptsImageInput,
                AcceptsFileInput = _editing.AcceptsFileInput,
                SupportsReasoning = _editing.SupportsReasoning,
                ReasoningEfforts = _editing.ReasoningEfforts,
                DefaultReasoningEffort = _editing.DefaultReasoningEffort,
                CapabilitySource = _editing.CapabilitySource
            };
            SetInputPrice(_editing.InputPrice);
            SetOutputPrice(_editing.OutputPrice);
            _showOutputPrice = _editing.OutputPrice.HasValue || CatalogPriceQuote.Default(_editing.CapabilityType).ShowOutput;
            _useWorkspaceConnection = UsesWorkspaceConnection(_editing);
            _showManualModelId = false;
            _availableModels = new List<OpenAIModelDto>();
            _modelsLoaded = false;
        }
        else
        {
            _hasStoredApiKey = false;
            _useWorkspaceConnection = true;
            _showManualModelId = false;
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = WorkspaceId,
                CapabilityType = AICapabilityType.ChatCompletion,
                Priority = 0,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                MaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens
            };
            SetInputPrice(null);
            SetOutputPrice(null);
            ApplyDefaultPriceUnits(AICapabilityType.ChatCompletion);
            _availableModels = new List<OpenAIModelDto>();
            _modelsLoaded = false;
        }
    }

    private void SetInputPrice(decimal? value)
    {
        _inputPriceSpecified = value.HasValue;
        _inputPriceValue = value ?? 0m;
        _model.InputPrice = value;
    }

    private void SetOutputPrice(decimal? value)
    {
        _outputPriceSpecified = value.HasValue;
        _outputPriceValue = value ?? 0m;
        _model.OutputPrice = value;
    }

    private void OnCapabilityChanged(AICapabilityType value)
    {
        if (_readOnly || _isEditMode || _model.CapabilityType == value)
        {
            return;
        }

        _model.CapabilityType = value;
        _model.OpenAIApiMode = OpenAIApiMode.ChatCompletions;
        _model.ModelId = string.Empty;
        _showManualModelId = false;

        if (!ShowsUserSelectable)
        {
            _model.IsUserSelectable = false;
        }

        if (!ShowsEmbeddingDimensions)
        {
            _model.Dimensions = null;
        }

        ApplyDefaultPriceUnits(value);
        SetInputPrice(null);
        SetOutputPrice(null);

        UnloadModels();
        _ = LoadModelsAsync(announce: false);
    }

    private Task OnConnectionPathChanged(bool useWorkspaceConnection)
    {
        if (_readOnly || _useWorkspaceConnection == useWorkspaceConnection)
        {
            return Task.CompletedTask;
        }

        _useWorkspaceConnection = useWorkspaceConnection;
        if (useWorkspaceConnection)
        {
            _model.ApiEndpoint = null;
            _model.ApiKey = null;
        }

        UnloadModels();
        return LoadModelsAsync(announce: false);
    }

    private void ShowManualModelEntry() => _showManualModelId = true;

    private void UnloadModels()
    {
        _availableModels = new List<OpenAIModelDto>();
        _modelsLoaded = false;
    }

    private void OnModelIdChanged(string? value)
    {
        if (_readOnly)
        {
            return;
        }

        var next = value ?? string.Empty;
        var changed = !string.Equals(_model.ModelId, next, StringComparison.OrdinalIgnoreCase);
        _model.ModelId = next;
        var selected = SelectedCatalogModel;
        if (selected != null && (changed || string.IsNullOrWhiteSpace(_model.DisplayName)))
        {
            _model.DisplayName = string.IsNullOrWhiteSpace(selected.DisplayName) ? selected.Id : selected.DisplayName;
        }

        if (changed)
        {
            ApplyCatalogSuggestion(replaceFlags: true);
        }
    }

    private void ApplyCatalogSuggestion(bool replaceFlags)
    {
        var selected = SelectedCatalogModel;
        if (selected == null)
        {
            return;
        }

        if (_model.CapabilityType == AICapabilityType.ChatCompletion)
        {
            if (replaceFlags)
            {
                _model.AcceptsImageInput = selected.AcceptsImageInput;
                _model.AcceptsFileInput = selected.AcceptsFileInput;
                _model.SupportsReasoning = selected.SupportsReasoning;
                _model.CapabilitySource = ModelCapabilitySource.ExternalCatalog;
                ApplyReasoningSuggestion(selected);
                if (selected.ContextLength is int contextLength && contextLength > 0)
                {
                    _model.MaxContextTokens = contextLength;
                }
            }
            else
            {
                ApplyInheritedCapabilitySuggestions(selected);
            }
        }

        var fillPrices = replaceFlags ||
                         (!_isEditMode && !_inputPriceSpecified && !_outputPriceSpecified);
        if (!fillPrices)
        {
            return;
        }

        _model.InputPriceUnit = selected.SuggestedInputPriceUnit;
        _model.OutputPriceUnit = selected.SuggestedOutputPriceUnit;
        _showOutputPrice = selected.SuggestOutputPrice;
        SetInputPrice(selected.SuggestedInputPrice);
        SetOutputPrice(selected.SuggestOutputPrice ? selected.SuggestedOutputPrice : null);
    }

    private void ApplyInheritedCapabilitySuggestions(OpenAIModelDto selected)
    {
        var filled = false;
        if (_model.AcceptsImageInput == null && selected.AcceptsImageInput != null)
        {
            _model.AcceptsImageInput = selected.AcceptsImageInput;
            filled = true;
        }

        if (_model.AcceptsFileInput == null && selected.AcceptsFileInput != null)
        {
            _model.AcceptsFileInput = selected.AcceptsFileInput;
            filled = true;
        }

        if (_model.SupportsReasoning == null && selected.SupportsReasoning != null)
        {
            _model.SupportsReasoning = selected.SupportsReasoning;
            filled = true;
        }

        if (_model.SupportsReasoning != false && string.IsNullOrWhiteSpace(_model.ReasoningEfforts))
        {
            ApplyReasoningSuggestion(selected);
            filled = !string.IsNullOrWhiteSpace(_model.ReasoningEfforts) || filled;
        }
        else if (string.IsNullOrWhiteSpace(_model.DefaultReasoningEffort) && AllowedEfforts.Count > 0)
        {
            _model.DefaultReasoningEffort = ReasoningEffortSelection.ChooseDefault(
                AllowedEfforts,
                selected.DefaultReasoningEffort);
        }

        if (filled && _model.CapabilitySource == null)
        {
            _model.CapabilitySource = ModelCapabilitySource.ExternalCatalog;
        }
    }

    private void ApplyReasoningSuggestion(OpenAIModelDto selected)
    {
        var efforts = selected.ReasoningEfforts ?? new List<string>();
        if (efforts.Count == 0)
        {
            _model.ReasoningEfforts = null;
            _model.DefaultReasoningEffort = null;
            return;
        }

        _model.ReasoningEfforts = string.Join(",", efforts);
        _model.DefaultReasoningEffort = ReasoningEffortSelection.ChooseDefault(efforts, selected.DefaultReasoningEffort);
    }

    private bool IsEffortAllowed(string effort) =>
        AllowedEfforts.Contains(effort, StringComparer.OrdinalIgnoreCase);

    private void ToggleEffort(string effort)
    {
        if (_readOnly)
        {
            return;
        }

        var allowed = AllowedEfforts.ToList();
        var existing = allowed.FirstOrDefault(item => item.Equals(effort, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            allowed.Add(effort);
        }
        else
        {
            allowed.Remove(existing);
        }

        _model.ReasoningEfforts = allowed.Count == 0 ? null : string.Join(",", allowed);
        _model.SupportsReasoning = allowed.Count > 0;
        _model.CapabilitySource = ModelCapabilitySource.Manual;
        if (!IsEffortAllowed(_model.DefaultReasoningEffort ?? string.Empty))
        {
            _model.DefaultReasoningEffort = ReasoningEffortSelection.ChooseDefault(allowed, providerDefault: null);
        }
    }

    private void SelectDefaultEffort(string effort)
    {
        if (_readOnly)
        {
            return;
        }

        if (!IsEffortAllowed(effort))
        {
            ToggleEffort(effort);
        }

        _model.DefaultReasoningEffort = effort;
        _model.CapabilitySource = ModelCapabilitySource.Manual;
    }

    private static List<string> SplitEfforts(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new List<string>();
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddEfforts(List<string> target, IEnumerable<string>? efforts)
    {
        if (efforts == null)
        {
            return;
        }

        foreach (var effort in efforts)
        {
            if (!string.IsNullOrWhiteSpace(effort) &&
                !target.Contains(effort, StringComparer.OrdinalIgnoreCase))
            {
                target.Add(effort.Trim());
            }
        }
    }

    private async Task SaveConfigurationAsync()
    {
        if (_readOnly)
        {
            return;
        }

        if (WorkspaceId == Guid.Empty)
        {
            await Message.ErrorAsync(L["WorkspaceRequired"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.ModelId))
        {
            await Message.ErrorAsync(L["ModelIdRequired"]);
            return;
        }

        if (_model.Priority < 0)
        {
            await Message.ErrorAsync(L["PriorityMustBeNumber"]);
            return;
        }

        _model.WorkspaceId = WorkspaceId;
        if (!ShowsUserSelectable)
        {
            _model.IsUserSelectable = false;
        }

        if (_useWorkspaceConnection)
        {
            _model.ApiEndpoint = null;
            _model.ApiKey = null;
        }
        else
        {
            _model.ApiEndpoint = string.IsNullOrWhiteSpace(_model.ApiEndpoint) ? null : _model.ApiEndpoint.Trim();
        }

        if (!TryApplyOpenAIApiMode() || !TryApplyMaxContextTokens() || !TryApplyPricing() || !TryApplyDimensions())
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            if (_isEditMode && _editing != null)
            {
                var updateDto = new UpdateAIModelConfigurationDto
                {
                    ModelId = _model.ModelId,
                    DisplayName = _model.DisplayName,
                    Description = _model.Description,
                    IsUserSelectable = _model.IsUserSelectable,
                    ApiEndpoint = _model.ApiEndpoint,
                    ApiKey = _model.ApiKey,
                    ClearApiKey = _useWorkspaceConnection,
                    OpenAIApiMode = _model.OpenAIApiMode,
                    MaxContextTokens = _model.MaxContextTokens,
                    InputPrice = _model.InputPrice,
                    InputPriceUnit = _model.InputPriceUnit,
                    OutputPrice = _showOutputPrice ? _model.OutputPrice : null,
                    OutputPriceUnit = _model.OutputPriceUnit,
                    Priority = _model.Priority,
                    Dimensions = _model.Dimensions,
                    AcceptsImageInput = _model.AcceptsImageInput,
                    AcceptsFileInput = _model.AcceptsFileInput,
                    SupportsReasoning = _model.SupportsReasoning,
                    ReasoningEfforts = _model.ReasoningEfforts,
                    DefaultReasoningEffort = _model.DefaultReasoningEffort,
                    CapabilitySource = _model.CapabilitySource
                };
                await AIAppService.UpdateModelConfigurationAsync(_editing.Id, updateDto);
                await Message.SuccessAsync(L["ConfigurationUpdated"]);
            }
            else
            {
                await AIAppService.CreateModelConfigurationAsync(_model);
                await Message.SuccessAsync(L["ConfigurationCreated"]);
            }

            Cancel();
        }, LoadingKeys.SaveConfiguration);
    }

    private async Task TestConnectionAsync()
    {
        if (_readOnly)
        {
            return;
        }

        if (WorkspaceId == Guid.Empty)
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
                WorkspaceId = WorkspaceId,
                ModelConfigurationId = ConnectionConfigurationId,
                CapabilityType = _model.CapabilityType,
                Model = _model.ModelId,
                ApiKey = ConnectionApiKey,
                ApiBaseUrl = ConnectionBaseUrl,
                OpenAIApiMode = _model.OpenAIApiMode
            });
            await Notify.SuccessAsync(L["ConnectionTestSuccessful"]);
        }, LoadingKeys.TestConnection);
    }

    private Task LoadModelsAsync() => LoadModelsAsync(announce: true);

    private async Task LoadModelsAsync(bool announce)
    {
        if (WorkspaceId == Guid.Empty)
        {
            if (announce)
            {
                await Notify.ErrorAsync(L["WorkspaceRequired"]);
            }

            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _availableModels = await WorkspaceAppService.GetAvailableModelsAsync(new GetOpenAIModelsInput
                {
                    WorkspaceId = WorkspaceId,
                    ModelConfigurationId = ConnectionConfigurationId,
                    ApiKey = ConnectionApiKey,
                    ApiBaseUrl = ConnectionBaseUrl,
                    CapabilityType = _model.CapabilityType
                });
            }
            catch (Exception ex)
            {
                _modelsLoaded = true;
                await Notify.ErrorAsync(ex.Message);
                return;
            }

            _modelsLoaded = true;
            if (_availableModels.Count == 0)
            {
                _showManualModelId = true;
                await Notify.WarnAsync(L["NoModelsMatchCapability"]);
                return;
            }

            if (!string.IsNullOrWhiteSpace(_model.ModelId))
            {
                ApplyCatalogSuggestion(replaceFlags: !_isEditMode);
            }

            if (announce)
            {
                await Notify.SuccessAsync(L["ModelsLoadedSuccessfully"]);
            }
        }, LoadingKeys.LoadModels);
    }

    private void Cancel()
    {
        Navigation.NavigateTo($"/panel/admin/ai/workspaces/{WorkspaceId}/model-configurations");
    }

    private bool TryApplyOpenAIApiMode()
    {
        _model.OpenAIApiMode = OpenAIApiMode.ChatCompletions;
        return true;
    }

    private bool TryApplyMaxContextTokens()
    {
        if (_model.MaxContextTokens < 1)
        {
            _model.MaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens;
        }

        return true;
    }

    private void ApplyDefaultPriceUnits(AICapabilityType capability)
    {
        var defaults = CatalogPriceQuote.Default(capability);
        _model.InputPriceUnit = defaults.InputUnit;
        _model.OutputPriceUnit = defaults.OutputUnit;
        _showOutputPrice = defaults.ShowOutput;
    }

    private string InputPriceLabel => PriceLabel(_model.InputPriceUnit, output: false);

    private string OutputPriceLabel => PriceLabel(_model.OutputPriceUnit, output: true);

    private string InputPricePlaceholder =>
        _model.InputPriceUnit == AIPriceUnit.PerMillionTokens ? L["InheritWorkspace"] : string.Empty;

    private string OutputPricePlaceholder =>
        _model.OutputPriceUnit == AIPriceUnit.PerMillionTokens ? L["InheritWorkspace"] : string.Empty;

    private string InputPriceHelp => PriceHelp(_model.InputPriceUnit);

    private string OutputPriceHelp => PriceHelp(_model.OutputPriceUnit);

    private string? EquivalentPriceText
    {
        get
        {
            if (!_inputPriceSpecified)
            {
                return null;
            }

            return _model.InputPriceUnit switch
            {
                AIPriceUnit.PerMinute => string.Format(CultureInfo.CurrentCulture, L["EquivalentHourlyPrice"], (_inputPriceValue * 60m).ToString("0.####", CultureInfo.InvariantCulture)),
                AIPriceUnit.PerHour => string.Format(CultureInfo.CurrentCulture, L["EquivalentMinutePrice"], (_inputPriceValue / 60m).ToString("0.####", CultureInfo.InvariantCulture)),
                _ => null
            };
        }
    }

    private string PriceLabel(AIPriceUnit unit, bool output) => unit switch
    {
        AIPriceUnit.PerMinute => L["CostPerMinute"],
        AIPriceUnit.PerHour => L["CostPerHour"],
        AIPriceUnit.PerMillionCharacters => L["CostPerMillionCharacters"],
        AIPriceUnit.PerImage => L["CostPerImage"],
        AIPriceUnit.PerRequest => L["CostPerRequest"],
        _ => output ? L["OutputCostPer1MTokens"] : L["InputCostPer1MTokens"]
    };

    private string PriceHelp(AIPriceUnit unit) => unit switch
    {
        AIPriceUnit.PerMinute => L["CostPerMinuteHelp"],
        AIPriceUnit.PerHour => L["CostPerHourHelp"],
        AIPriceUnit.PerMillionCharacters => L["CostPerMillionCharactersHelp"],
        AIPriceUnit.PerImage => L["CostPerImageHelp"],
        AIPriceUnit.PerRequest => L["CostPerRequestHelp"],
        _ => L["RoutePriceWinsHelp"]
    };

    private bool TryApplyPricing()
    {
        if (_inputPriceSpecified && _inputPriceValue < 0)
        {
            _ = Message.ErrorAsync(L["InputPriceMustBeNonNegative"]);
            return false;
        }

        if (_showOutputPrice && _outputPriceSpecified && _outputPriceValue < 0)
        {
            _ = Message.ErrorAsync(L["OutputPriceMustBeNonNegative"]);
            return false;
        }

        _model.InputPrice = _inputPriceSpecified ? _inputPriceValue : null;
        _model.OutputPrice = _showOutputPrice && _outputPriceSpecified ? _outputPriceValue : null;
        return true;
    }

    private bool TryApplyDimensions()
    {
        if (!ShowsEmbeddingDimensions || _model.Dimensions is not int dimensions || dimensions <= 0)
        {
            _model.Dimensions = null;
        }

        return true;
    }
}
