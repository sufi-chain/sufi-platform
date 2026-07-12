using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Unified AI service implementation that orchestrates multiple providers and capabilities
/// </summary>
public class AIService : DomainService, IAIService, ITransientDependency
{
    private const string ProviderDidNotReturnUsage = "ProviderDidNotReturnUsage";
    private const string UsageUnavailable = "UsageUnavailable";
    private const string PricingNotConfigured = "PricingNotConfigured";

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IAIModelConfigurationRepository _configurationRepository;
    private readonly IAIUsageLogRepository _usageLogRepository;
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<AIService> _logger;

    public AIService(
        IWorkspaceRepository workspaceRepository,
        IAIModelConfigurationRepository configurationRepository,
        IAIUsageLogRepository usageLogRepository,
        IEnumerable<IAIProvider> providers,
        IFeatureChecker featureChecker,
        ILogger<AIService> logger)
    {
        _workspaceRepository = workspaceRepository;
        _configurationRepository = configurationRepository;
        _usageLogRepository = usageLogRepository;
        _providers = providers;
        _featureChecker = featureChecker;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "AI chat completion requested. WorkspaceName={WorkspaceName}, MessageCount={MessageCount}, HasSystemPrompt={HasSystemPrompt}, Stream={Stream}",
            request.WorkspaceName,
            request.Messages.Count,
            !string.IsNullOrWhiteSpace(request.SystemPrompt),
            request.Stream);

        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.ChatCompletion,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogDebug(
                "AI provider chat completion started. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Model={Model}, Capability={Capability}",
                workspace.Id,
                workspace.Name,
                workspace.Provider,
                configuration.ModelId,
                AICapabilityType.ChatCompletion);

            var response = await provider.SendChatMessageAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            _logger.LogDebug(
                "AI provider chat completion completed. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Model={Model}, ContentLength={ContentLength}, TotalTokens={TotalTokens}, LatencyMs={LatencyMs}",
                workspace.Id,
                workspace.Name,
                workspace.Provider,
                response.ModelId,
                response.Content?.Length ?? 0,
                response.TotalTokens,
                stopwatch.ElapsedMilliseconds);

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.ChatCompletion,
                response.InputTokens,
                response.OutputTokens,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                ex,
                "AI provider chat completion failed. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Model={Model}, LatencyMs={LatencyMs}",
                workspace.Id,
                workspace.Name,
                workspace.Provider,
                configuration.ModelId,
                stopwatch.ElapsedMilliseconds);

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.ChatCompletion,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.ChatCompletion,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        int? inputTokens = null;
        int? outputTokens = null;
        int? totalTokens = null;
        string? usageUnavailableReason = ProviderDidNotReturnUsage;
        var hasError = false;
        var errorMessage = string.Empty;

        var stream = provider.StreamChatMessageAsync(workspace, configuration, request, cancellationToken);
        var enumerator = stream.GetAsyncEnumerator(cancellationToken);

        try
        {
            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                }
                catch (Exception ex)
                {
                    hasError = true;
                    errorMessage = ex.Message;
                    _logger.LogError(ex, "Error during streaming chat completion");
                    throw;
                }

                if (!hasNext)
                {
                    break;
                }

                var chunk = enumerator.Current;
                if (chunk.IsUsageChunk)
                {
                    inputTokens = chunk.InputTokens;
                    outputTokens = chunk.OutputTokens;
                    totalTokens = chunk.TotalTokens;
                    usageUnavailableReason = chunk.UsageUnavailableReason;
                    continue;
                }

                yield return chunk;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
            stopwatch.Stop();

            try
            {
                await LogUsageAsync(
                    workspace,
                    configuration,
                    AICapabilityType.ChatCompletion,
                    inputTokens,
                    outputTokens,
                    totalTokens,
                    usageUnavailableReason,
                    stopwatch.ElapsedMilliseconds,
                    isSuccess: !hasError,
                    errorMessage: hasError ? errorMessage : null,
                    cancellationToken: cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log usage for streaming chat completion");
            }
        }
    }

    public async Task<AudioTranscriptionResponse> TranscribeAudioAsync(
        AudioTranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.AudioTranscription,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.TranscribeAudioAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.AudioTranscription,
                response.InputTokens,
                response.OutputTokens,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.AudioTranscription,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.TextToSpeech,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.GenerateSpeechAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.TextToSpeech,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.TextToSpeech,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<VisionAnalysisResponse> AnalyzeImageAsync(
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.VisionAnalysis,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.AnalyzeImageAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.VisionAnalysis,
                response.InputTokens,
                response.OutputTokens,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.VisionAnalysis,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.Embeddings,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.GenerateEmbeddingsAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.Embeddings,
                response.TotalTokens,
                response.TotalTokens.HasValue ? 0 : null,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                workspace,
                configuration,
                AICapabilityType.Embeddings,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message,
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<bool> HasCapabilityAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (workspace == null) return false;

        return workspace.HasCapability(capabilityType);
    }

    private async Task<(Workspace workspace, AIModelConfiguration configuration, IAIProvider provider)> PrepareRequestAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken)
    {
        await CheckFeatureAsync(capabilityType);
        _logger.LogDebug(
            "Preparing AI request. WorkspaceName={WorkspaceName}, Capability={Capability}",
            workspaceName,
            capabilityType);

        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (workspace == null)
        {
            _logger.LogDebug(
                "AI workspace not found. WorkspaceName={WorkspaceName}, Capability={Capability}",
                workspaceName,
                capabilityType);
            throw new BusinessException("AI:WorkspaceNotFound")
                .WithData("WorkspaceName", workspaceName);
        }

        if (!workspace.IsActive)
        {
            _logger.LogDebug(
                "AI workspace is inactive. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Capability={Capability}",
                workspace.Id,
                workspace.Name,
                capabilityType);
            throw new BusinessException("AI:WorkspaceNotActive")
                .WithData("WorkspaceName", workspaceName);
        }

        var configuration = await _configurationRepository.GetPrimaryConfigurationAsync(
            workspace.Id,
            capabilityType,
            cancellationToken);

        if (configuration == null)
        {
            var fallbackModelId = workspace.DefaultModel;

            if (string.IsNullOrWhiteSpace(fallbackModelId))
            {
                fallbackModelId = capabilityType switch
                {
                    AICapabilityType.ChatCompletion => "gpt-3.5-turbo",
                    AICapabilityType.AudioTranscription => "whisper-1",
                    AICapabilityType.TextToSpeech => "tts-1",
                    AICapabilityType.Embeddings => "text-embedding-3-small",
                    AICapabilityType.ImageGeneration => "dall-e-3",
                    _ => null
                };
            }

            if (string.IsNullOrWhiteSpace(fallbackModelId))
            {
                throw new BusinessException("AI:NoModelConfigured")
                    .WithData("WorkspaceName", workspaceName)
                    .WithData("CapabilityType", capabilityType.ToString());
            }

            configuration = new AIModelConfiguration(
                Guid.NewGuid(),
                workspace.Id,
                capabilityType,
                fallbackModelId,
                priority: 999);

            configuration.UpdateConfiguration(
                fallbackModelId,
                workspace.ApiBaseUrl,
                workspace.ApiKey,
                null,
                999,
                workspace.OpenAIApiMode,
                workspace.InputCostPer1MTokens,
                workspace.OutputCostPer1MTokens);

            _logger.LogDebug(
                "AI request using workspace fallback model configuration. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Capability={Capability}, Model={Model}",
                workspace.Id,
                workspace.Name,
                capabilityType,
                configuration.ModelId);
        }
        else
        {
            _logger.LogDebug(
                "AI request using primary model configuration. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Capability={Capability}, Model={Model}, ConfigurationId={ConfigurationId}",
                workspace.Id,
                workspace.Name,
                capabilityType,
                configuration.ModelId,
                configuration.Id);
        }

        var provider = _providers.FirstOrDefault(p => p.ProviderType == workspace.Provider);
        if (provider == null)
        {
            _logger.LogDebug(
                "AI provider not registered. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}",
                workspace.Id,
                workspace.Name,
                workspace.Provider);
            throw new BusinessException("AI:ProviderNotSupported")
                .WithData("Provider", workspace.Provider.ToString());
        }

        if (!provider.SupportsCapability(capabilityType))
        {
            _logger.LogDebug(
                "AI provider does not support capability. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Capability={Capability}",
                workspace.Id,
                workspace.Name,
                workspace.Provider,
                capabilityType);
            throw new BusinessException("AI:CapabilityNotSupported")
                .WithData("Provider", workspace.Provider.ToString())
                .WithData("CapabilityType", capabilityType.ToString());
        }

        _logger.LogDebug(
            "AI request prepared. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Capability={Capability}, Model={Model}",
            workspace.Id,
            workspace.Name,
            workspace.Provider,
            capabilityType,
            configuration.ModelId);

        return (workspace, configuration, provider);
    }

    private async Task CheckFeatureAsync(AICapabilityType capabilityType)
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAIFeatures.Enable}");
        }

        var featureName = capabilityType switch
        {
            AICapabilityType.ChatCompletion => SufiAIFeatures.Chat,
            AICapabilityType.AudioTranscription => SufiAIFeatures.Audio,
            AICapabilityType.TextToSpeech => SufiAIFeatures.Audio,
            AICapabilityType.VisionAnalysis => SufiAIFeatures.Vision,
            AICapabilityType.Embeddings => SufiAIFeatures.Embeddings,
            AICapabilityType.ImageGeneration => SufiAIFeatures.Vision,
            _ => SufiAIFeatures.Enable
        };

        if (!await _featureChecker.IsEnabledAsync(featureName))
        {
            throw new BusinessException($"Feature is disabled: {featureName}");
        }
    }

    private async Task LogUsageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        AICapabilityType capabilityType,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        string? usageUnavailableReason,
        long latencyMs,
        bool isSuccess,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new AIUsageLog(
                GuidGenerator.Create(),
                workspace.Id,
                capabilityType,
                configuration.ModelId,
                workspace.Provider,
                workspace.TenantId);

            if (isSuccess)
            {
                var cost = CalculateCost(workspace, configuration, inputTokens, outputTokens, totalTokens);
                log.RecordSuccess(
                    inputTokens,
                    outputTokens,
                    latencyMs,
                    cost.EstimatedCost,
                    totalTokens,
                    cost.IsCostCalculated,
                    usageUnavailableReason,
                    cost.CostCalculationNote);
            }
            else
            {
                log.RecordFailure(errorMessage ?? "Unknown error", latencyMs);
            }

            await _usageLogRepository.InsertAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log AI usage for workspace {WorkspaceName}", workspace.Name);
        }
    }

    private static CostCalculationResult CalculateCost(
        Workspace workspace,
        AIModelConfiguration configuration,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens)
    {
        var hasTokenUsage = inputTokens.HasValue || outputTokens.HasValue || totalTokens.HasValue;
        if (!hasTokenUsage)
        {
            return new CostCalculationResult(0, false, UsageUnavailable);
        }

        var inputCostPer1MTokens = configuration.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens;
        var outputCostPer1MTokens = configuration.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens;
        var hasPricing = inputCostPer1MTokens.HasValue || outputCostPer1MTokens.HasValue;

        if (!hasPricing)
        {
            return new CostCalculationResult(0, false, PricingNotConfigured);
        }

        var estimatedCost = ((inputTokens ?? 0) * (inputCostPer1MTokens ?? 0) +
                             (outputTokens ?? 0) * (outputCostPer1MTokens ?? 0)) / 1000000m;

        return new CostCalculationResult(estimatedCost, true, null);
    }

    private sealed record CostCalculationResult(
        decimal EstimatedCost,
        bool IsCostCalculated,
        string? CostCalculationNote);
}
