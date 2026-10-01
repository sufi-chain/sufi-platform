using System;
using System.Globalization;
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
    [Parameter] public AIProviderType WorkspaceProvider { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private bool _isEditMode;
    private bool _hasStoredApiKey;
    private bool _useWorkspaceConnection = true;
    private int _formKey;
    private bool _showManualModelId;
    private CreateAIModelConfigurationDto _model = new();
    private string _priorityText = "0";
    private string _maxContextTokensText = "200000";
    private string _inputPriceText = string.Empty;
    private string _outputPriceText = string.Empty;
    private bool _showOutputPrice = true;
    private string _dimensionsText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private bool _modelsLoaded;
    private bool _wasOpen;

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

    private Guid? ConnectionConfigurationId => _useWorkspaceConnection ? null : Configuration?.Id;

    private IReadOnlyList<OpenAIModelDto> SelectableModels =>
        WorkspaceConnectionDraft.WithCurrent(_availableModels, _model.ModelId, _model.DisplayName);

    private List<AiProviderProfileDto> _profiles = new();

    private string ProviderDisplayName =>
        _profiles.FirstOrDefault(profile => profile.ProviderType == WorkspaceProvider)?.DisplayName
        ?? WorkspaceProvider.ToString();

    private bool ShowsUserSelectable =>
        _model.CapabilityType == AICapabilityType.ChatCompletion;

    private bool ShowsEmbeddingDimensions =>
        _model.CapabilityType == AICapabilityType.Embeddings;

    private bool ModelChanged =>
        _isEditMode &&
        Configuration != null &&
        !string.Equals(Configuration.ModelId?.Trim(), _model.ModelId?.Trim(), StringComparison.OrdinalIgnoreCase);

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

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            _wasOpen = true;
            _isEditMode = Configuration != null;
            ResetForm();
            _ = LoadModelsAsync(announce: false);
        }

        if (Open && _profiles.Count == 0)
        {
            _ = LoadProfilesAsync();
        }

        _wasOpen = Open;
    }

    private Task LoadProfilesAsync()
    {
        return ExecuteWithLoadingAsync(async () =>
        {
            _profiles = await WorkspaceAppService.GetProviderProfilesAsync();
        });
    }

    private static bool UsesWorkspaceConnection(AIModelConfigurationDto configuration) =>
        string.IsNullOrWhiteSpace(configuration.ApiEndpoint) && !configuration.HasApiKey;

    private void ResetForm()
    {
        _formKey++;
        if (_isEditMode && Configuration != null)
        {
            _hasStoredApiKey = Configuration.HasApiKey;
            _model = new CreateAIModelConfigurationDto
            {
                WorkspaceId = Configuration.WorkspaceId,
                CapabilityType = Configuration.CapabilityType,
                ModelId = Configuration.ModelId,
                DisplayName = Configuration.DisplayName,
                Description = Configuration.Description,
                IsUserSelectable = Configuration.IsUserSelectable,
                ApiEndpoint = Configuration.ApiEndpoint,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                MaxContextTokens = Configuration.MaxContextTokens > 0
                    ? Configuration.MaxContextTokens
                    : AIModelConfigurationConsts.DefaultMaxContextTokens,
                InputPrice = Configuration.InputPrice,
                InputPriceUnit = Configuration.InputPriceUnit,
                OutputPrice = Configuration.OutputPrice,
                OutputPriceUnit = Configuration.OutputPriceUnit,
                Priority = Configuration.Priority,
                Dimensions = Configuration.Dimensions,
                AcceptsImageInput = Configuration.AcceptsImageInput,
                AcceptsFileInput = Configuration.AcceptsFileInput,
                SupportsReasoning = Configuration.SupportsReasoning,
                ReasoningEfforts = Configuration.ReasoningEfforts,
                DefaultReasoningEffort = Configuration.DefaultReasoningEffort,
                CapabilitySource = Configuration.CapabilitySource
            };
            _priorityText = Configuration.Priority.ToString(CultureInfo.InvariantCulture);
            _maxContextTokensText = _model.MaxContextTokens.ToString(CultureInfo.InvariantCulture);
            _inputPriceText = Configuration.InputPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _outputPriceText = Configuration.OutputPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _showOutputPrice = Configuration.OutputPrice.HasValue || CatalogPriceQuote.Default(Configuration.CapabilityType).ShowOutput;
            _dimensionsText = Configuration.Dimensions?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _useWorkspaceConnection = UsesWorkspaceConnection(Configuration);
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
                WorkspaceId = WorkspaceId ?? Guid.Empty,
                CapabilityType = AICapabilityType.ChatCompletion,
                Priority = 0,
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                MaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens
            };
            _priorityText = "0";
            _maxContextTokensText = AIModelConfigurationConsts.DefaultMaxContextTokens.ToString(CultureInfo.InvariantCulture);
            _inputPriceText = string.Empty;
            _outputPriceText = string.Empty;
            ApplyDefaultPriceUnits(AICapabilityType.ChatCompletion);
            _dimensionsText = string.Empty;
            _availableModels = new List<OpenAIModelDto>();
            _modelsLoaded = false;
        }
    }

    private void OnCapabilityChanged(AICapabilityType value)
    {
        if (_isEditMode || _model.CapabilityType == value)
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
            _dimensionsText = string.Empty;
            _model.Dimensions = null;
        }

        ApplyDefaultPriceUnits(value);
        _inputPriceText = string.Empty;
        _outputPriceText = string.Empty;
        _model.InputPrice = null;
        _model.OutputPrice = null;

        UnloadModels();
        _ = LoadModelsAsync(announce: false);
    }

    private Task OnConnectionPathChanged(bool useWorkspaceConnection)
    {
        if (_useWorkspaceConnection == useWorkspaceConnection)
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
                    _maxContextTokensText = contextLength.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                ApplyInheritedCapabilitySuggestions(selected);
            }
        }

        var fillPrices = replaceFlags ||
                         (!_isEditMode &&
                          string.IsNullOrWhiteSpace(_inputPriceText) &&
                          string.IsNullOrWhiteSpace(_outputPriceText));
        if (!fillPrices)
        {
            return;
        }

        _model.InputPriceUnit = selected.SuggestedInputPriceUnit;
        _model.OutputPriceUnit = selected.SuggestedOutputPriceUnit;
        _showOutputPrice = selected.SuggestOutputPrice;
        _inputPriceText = selected.SuggestedInputPrice?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;
        _model.InputPrice = selected.SuggestedInputPrice;
        if (selected.SuggestOutputPrice)
        {
            _outputPriceText = selected.SuggestedOutputPrice?.ToString("0.####", CultureInfo.InvariantCulture) ?? string.Empty;
            _model.OutputPrice = selected.SuggestedOutputPrice;
        }
        else
        {
            _outputPriceText = string.Empty;
            _model.OutputPrice = null;
        }
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
            if (_isEditMode && Configuration != null)
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
        if (!WorkspaceId.HasValue)
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
                    WorkspaceId = WorkspaceId.Value,
                    ModelConfigurationId = ConnectionConfigurationId,
                    ApiKey = ConnectionApiKey,
                    ApiBaseUrl = ConnectionBaseUrl,
                    CapabilityType = _model.CapabilityType
                });
            }
            catch (Exception ex)
            {
                _modelsLoaded = true;
                if (announce || Open)
                {
                    await Notify.ErrorAsync(ex.Message);
                }

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
        _model.OpenAIApiMode = OpenAIApiMode.ChatCompletions;
        return true;
    }

    private bool TryApplyMaxContextTokens()
    {
        if (string.IsNullOrWhiteSpace(_maxContextTokensText))
        {
            _model.MaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens;
            return true;
        }

        if (!int.TryParse(_maxContextTokensText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tokens)
            && !int.TryParse(_maxContextTokensText, NumberStyles.Integer, CultureInfo.CurrentCulture, out tokens))
        {
            _ = Message.ErrorAsync(L["MaxContextTokensMustBeNumber"]);
            return false;
        }

        if (tokens < 1)
        {
            _model.MaxContextTokens = AIModelConfigurationConsts.DefaultMaxContextTokens;
            return true;
        }

        _model.MaxContextTokens = tokens;
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
            if (!TryParseNullableDecimal(_inputPriceText, out var amount) || amount is not decimal price)
            {
                return null;
            }

            return _model.InputPriceUnit switch
            {
                AIPriceUnit.PerMinute => string.Format(CultureInfo.CurrentCulture, L["EquivalentHourlyPrice"], (price * 60m).ToString("0.####", CultureInfo.InvariantCulture)),
                AIPriceUnit.PerHour => string.Format(CultureInfo.CurrentCulture, L["EquivalentMinutePrice"], (price / 60m).ToString("0.####", CultureInfo.InvariantCulture)),
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
        if (!TryParseNullableDecimal(_inputPriceText, out var inputCost))
        {
            _ = Message.ErrorAsync(L["InputPriceMustBeNonNegative"]);
            return false;
        }

        decimal? outputCost = null;
        if (_showOutputPrice && !TryParseNullableDecimal(_outputPriceText, out outputCost))
        {
            _ = Message.ErrorAsync(L["OutputPriceMustBeNonNegative"]);
            return false;
        }

        _model.InputPrice = inputCost;
        _model.OutputPrice = _showOutputPrice ? outputCost : null;
        return true;
    }

    private bool TryApplyDimensions()
    {
        if (!ShowsEmbeddingDimensions)
        {
            _model.Dimensions = null;
            return true;
        }

        if (string.IsNullOrWhiteSpace(_dimensionsText))
        {
            _model.Dimensions = null;
            return true;
        }

        if (int.TryParse(_dimensionsText, out var dimensions) && dimensions > 0)
        {
            _model.Dimensions = dimensions;
            return true;
        }

        _ = Message.ErrorAsync(L["InvalidEmbeddingDimensions"]);
        return false;
    }

    private static bool TryParseNullableDecimal(string? value, out decimal? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if ((decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
             || decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsed))
            && parsed >= 0)
        {
            result = parsed;
            return true;
        }

        return false;
    }
}
