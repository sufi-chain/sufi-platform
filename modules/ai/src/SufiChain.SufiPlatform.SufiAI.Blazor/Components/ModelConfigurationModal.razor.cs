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
    [Parameter] public EventCallback OnSaved { get; set; }

    private IAIAppService AIAppService => LazyGetRequiredService(ref _aiAppService);
    private IAIAppService? _aiAppService;

    private bool _isEditMode;
    private bool _hasStoredApiKey;
    private CreateAIModelConfigurationDto _model = new();
    private string _priorityText = "0";
    private string _maxContextTokensText = "200000";
    private string _inputCostPer1MTokensText = string.Empty;
    private string _outputCostPer1MTokensText = string.Empty;
    private string _dimensionsText = string.Empty;
    private List<OpenAIModelDto> _availableModels = new();
    private bool _wasOpen;

    private bool ShowsOpenAIApiMode =>
        _model.CapabilityType is AICapabilityType.ChatCompletion or AICapabilityType.VisionAnalysis;

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

    private OpenAIPublicListPrice? SuggestedListPrice =>
        OpenAIPublicListPriceCatalog.TryGet(_model.ModelId, out var price) ? price : null;

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
                DisplayName = Configuration.DisplayName,
                Description = Configuration.Description,
                IsUserSelectable = Configuration.IsUserSelectable,
                ApiEndpoint = Configuration.ApiEndpoint,
                OpenAIApiMode = Configuration.OpenAIApiMode,
                MaxContextTokens = Configuration.MaxContextTokens > 0
                    ? Configuration.MaxContextTokens
                    : AIModelConfigurationConsts.DefaultMaxContextTokens,
                InputCostPer1MTokens = Configuration.InputCostPer1MTokens,
                OutputCostPer1MTokens = Configuration.OutputCostPer1MTokens,
                Priority = Configuration.Priority,
                Dimensions = Configuration.Dimensions
            };
            _priorityText = Configuration.Priority.ToString(CultureInfo.InvariantCulture);
            _maxContextTokensText = _model.MaxContextTokens.ToString(CultureInfo.InvariantCulture);
            _inputCostPer1MTokensText = Configuration.InputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _outputCostPer1MTokensText = Configuration.OutputCostPer1MTokens?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _dimensionsText = Configuration.Dimensions?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _availableModels = new List<OpenAIModelDto>
            {
                new() { Id = Configuration.ModelId }
            };
        }
        else
        {
            _hasStoredApiKey = false;
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
            _inputCostPer1MTokensText = string.Empty;
            _outputCostPer1MTokensText = string.Empty;
            _dimensionsText = string.Empty;
            _availableModels = new List<OpenAIModelDto>();
        }
    }

    private void OnCapabilityChanged(AICapabilityType value)
    {
        _model.CapabilityType = value;
        if (!ShowsOpenAIApiMode)
        {
            _model.OpenAIApiMode = OpenAIApiMode.ChatCompletions;
        }

        if (!ShowsUserSelectable)
        {
            _model.IsUserSelectable = false;
        }

        if (!ShowsEmbeddingDimensions)
        {
            _dimensionsText = string.Empty;
            _model.Dimensions = null;
        }
    }

    private void OnModelIdChanged(string? value)
    {
        _model.ModelId = value ?? string.Empty;
        TryApplySuggestedListPriceIfEmpty();
    }

    private void ApplyOpenAIListPrice()
    {
        if (SuggestedListPrice is not { } listPrice)
        {
            return;
        }

        ApplyListPrice(listPrice);
    }

    private void TryApplySuggestedListPriceIfEmpty()
    {
        if (!string.IsNullOrWhiteSpace(_inputCostPer1MTokensText)
            || !string.IsNullOrWhiteSpace(_outputCostPer1MTokensText)
            || SuggestedListPrice is not { } listPrice)
        {
            return;
        }

        ApplyListPrice(listPrice);
    }

    private void ApplyListPrice(OpenAIPublicListPrice listPrice)
    {
        _inputCostPer1MTokensText = listPrice.InputCostPer1MTokens.ToString("0.####", CultureInfo.InvariantCulture);
        _outputCostPer1MTokensText = listPrice.OutputCostPer1MTokens?.ToString("0.####", CultureInfo.InvariantCulture)
            ?? string.Empty;
    }

    private string FormatOpenAIListPriceHint(OpenAIPublicListPrice listPrice)
    {
        var asOf = OpenAIPublicListPriceCatalog.AsOf;
        var input = listPrice.InputCostPer1MTokens.ToString("0.####", CultureInfo.InvariantCulture);
        if (!listPrice.OutputCostPer1MTokens.HasValue)
        {
            return L["OpenAIListPriceHintInputOnly", listPrice.ModelId, asOf, input];
        }

        var output = listPrice.OutputCostPer1MTokens.Value.ToString("0.####", CultureInfo.InvariantCulture);
        return L["OpenAIListPriceHint", listPrice.ModelId, asOf, input, output];
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
                    OpenAIApiMode = _model.OpenAIApiMode,
                    MaxContextTokens = _model.MaxContextTokens,
                    InputCostPer1MTokens = _model.InputCostPer1MTokens,
                    OutputCostPer1MTokens = _model.OutputCostPer1MTokens,
                    Priority = _model.Priority,
                    Dimensions = _model.Dimensions
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
                OpenAIApiMode = _model.OpenAIApiMode
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
                OnModelIdChanged(_availableModels[0].Id);
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
        if (!ShowsOpenAIApiMode)
        {
            _model.OpenAIApiMode = OpenAIApiMode.ChatCompletions;
            return true;
        }

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
