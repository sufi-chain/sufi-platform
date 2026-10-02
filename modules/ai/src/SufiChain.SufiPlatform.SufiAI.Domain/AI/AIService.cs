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

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly IAIUsageRecorder _usageRecorder;
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly IEnumerable<IAiProviderProfile> _profiles;
    private readonly IModelEndpointLookup _endpointLookup;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<AIService> _logger;

    public AIService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IAIUsageRecorder usageRecorder,
        IEnumerable<IAIProvider> providers,
        IEnumerable<IAiProviderProfile> profiles,
        IModelEndpointLookup endpointLookup,
        IFeatureChecker featureChecker,
        ILogger<AIService> logger)
    {
        _workspaceRepository = workspaceRepository;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _usageRecorder = usageRecorder;
        _providers = providers;
        _profiles = profiles;
        _endpointLookup = endpointLookup;
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

        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request,
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
            if (GuardModelReply.IsVerdict(response.Content))
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "Guard-model verdict returned as chat content. Retrying once. WorkspaceId={WorkspaceId}, ConfiguredModel={ConfiguredModel}, UpstreamModel={UpstreamModel}",
                    workspace.Id,
                    configuration.ModelId,
                    response.ModelId);
                await LogUsageAsync(
                    resolved,
                    AICapabilityType.ChatCompletion,
                    response.InputTokens,
                    response.OutputTokens,
                    response.TotalTokens,
                    response.UsageUnavailableReason,
                    stopwatch.ElapsedMilliseconds,
                    isSuccess: false,
                    errorMessage: AIErrorCodes.GuardModelReply,
                    modelId: response.ModelId,
                    cancellationToken: cancellationToken);

                stopwatch.Restart();
                response = await provider.SendChatMessageAsync(workspace, configuration, request, cancellationToken);
                stopwatch.Stop();
                if (GuardModelReply.IsVerdict(response.Content))
                {
                    await LogUsageAsync(
                        resolved,
                        AICapabilityType.ChatCompletion,
                        response.InputTokens,
                        response.OutputTokens,
                        response.TotalTokens,
                        response.UsageUnavailableReason,
                        stopwatch.ElapsedMilliseconds,
                        isSuccess: false,
                        errorMessage: AIErrorCodes.GuardModelReply,
                        modelId: response.ModelId,
                        cancellationToken: cancellationToken);
                    throw new BusinessException(AIErrorCodes.GuardModelReply)
                        .WithData("ConfiguredModel", configuration.ModelId)
                        .WithData("UpstreamModel", response.ModelId ?? string.Empty);
                }
            }
            else
            {
                stopwatch.Stop();
            }

            _logger.LogDebug(
                "AI provider chat completion completed. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}, Model={Model}, UpstreamModel={UpstreamModel}, ContentLength={ContentLength}, TotalTokens={TotalTokens}, LatencyMs={LatencyMs}",
                workspace.Id,
                workspace.Name,
                workspace.Provider,
                configuration.ModelId,
                response.ModelId,
                response.Content?.Length ?? 0,
                response.TotalTokens,
                stopwatch.ElapsedMilliseconds);

            await LogUsageAsync(
                resolved,
                AICapabilityType.ChatCompletion,
                response.InputTokens,
                response.OutputTokens,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                modelId: response.ModelId,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (BusinessException ex) when (ex.Code == AIErrorCodes.GuardModelReply)
        {
            throw;
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
                resolved,
                AICapabilityType.ChatCompletion,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: FormatLoggedError(ex),
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request,
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
        string? upstreamModel = null;

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
                    errorMessage = FormatLoggedError(ex);
                    _logger.LogError(ex, "Error during streaming chat completion");
                    throw;
                }

                if (!hasNext)
                {
                    break;
                }

                var chunk = enumerator.Current;
                if (!string.IsNullOrWhiteSpace(chunk.ModelId))
                {
                    upstreamModel = chunk.ModelId;
                }

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
                    resolved,
                    AICapabilityType.ChatCompletion,
                    inputTokens,
                    outputTokens,
                    totalTokens,
                    usageUnavailableReason,
                    stopwatch.ElapsedMilliseconds,
                    isSuccess: !hasError,
                    errorMessage: hasError ? errorMessage : null,
                    modelId: upstreamModel,
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
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.AudioTranscription,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.TranscribeAudioAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
                AICapabilityType.AudioTranscription,
                response.InputTokens,
                response.OutputTokens,
                response.TotalTokens,
                response.UsageUnavailableReason,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                audioSeconds: response.BilledSeconds,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
                AICapabilityType.AudioTranscription,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: FormatLoggedError(ex),
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.TextToSpeech,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.GenerateSpeechAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
                AICapabilityType.TextToSpeech,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                characterCount: request.Text?.Length,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
                AICapabilityType.TextToSpeech,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: FormatLoggedError(ex),
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<VisionAnalysisResponse> AnalyzeImageAsync(
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.VisionAnalysis,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.AnalyzeImageAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
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
                resolved,
                AICapabilityType.VisionAnalysis,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: FormatLoggedError(ex),
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.ImageGeneration,
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.GenerateImageAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.ImageGeneration, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, true, imageCount: 1, cancellationToken: cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.ImageGeneration, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, false, FormatLoggedError(ex), cancellationToken: cancellationToken);
            throw;
        }
    }

    public async Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.Embeddings,
            new AIModelRouteSelection
            {
                ModelConfigurationId = request.ModelConfigurationId
            },
            cancellationToken);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await provider.GenerateEmbeddingsAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();

            await LogUsageAsync(
                resolved,
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
                resolved,
                AICapabilityType.Embeddings,
                null,
                null,
                null,
                ProviderDidNotReturnUsage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: FormatLoggedError(ex),
                cancellationToken: cancellationToken);

            throw;
        }
    }

    public async Task<WebSearchResponse> SearchWebAsync(
        WebSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.WebSearch,
            cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await provider.SearchWebAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.WebSearch, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, true, cancellationToken: cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.WebSearch, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, false, FormatLoggedError(ex), cancellationToken: cancellationToken);
            throw;
        }
    }

    public async Task<WebFetchResponse> FetchWebAsync(
        WebFetchRequest request,
        CancellationToken cancellationToken = default)
    {
        var (workspace, configuration, provider, resolved) = await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.WebFetch,
            cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await provider.FetchWebAsync(workspace, configuration, request, cancellationToken);
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.WebFetch, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, true, cancellationToken: cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogUsageAsync(resolved, AICapabilityType.WebFetch, null, null, null,
                UsageUnavailable, stopwatch.ElapsedMilliseconds, false, FormatLoggedError(ex), cancellationToken: cancellationToken);
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

        return _runtimeConfigurationResolver.Resolve(workspace, capabilityType).IsConfigured;
    }

    private async Task<(Workspace workspace, AIModelConfiguration configuration, IAIProvider provider, WorkspaceRuntimeConfiguration resolved)> PrepareRequestAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        return await PrepareRequestAsync(
            request.WorkspaceName,
            AICapabilityType.ChatCompletion,
            new AIModelRouteSelection
            {
                ModelConfigurationId = request.ModelConfigurationId
            },
            cancellationToken,
            request.RequestedApiMode);
    }

    private async Task<(Workspace workspace, AIModelConfiguration configuration, IAIProvider provider, WorkspaceRuntimeConfiguration resolved)> PrepareRequestAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken)
    {
        return await PrepareRequestAsync(
            workspaceName,
            capabilityType,
            AIModelRouteSelection.Implicit,
            cancellationToken);
    }

    private async Task<(Workspace workspace, AIModelConfiguration configuration, IAIProvider provider, WorkspaceRuntimeConfiguration resolved)> PrepareRequestAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken,
        OpenAIApiMode? requestedApiMode = null)
    {
        await CheckFeatureAsync(capabilityType);
        _logger.LogDebug(
            "Preparing AI request. WorkspaceName={WorkspaceName}, Capability={Capability}, ModelConfigurationId={ModelConfigurationId}, IsExplicitSelection={IsExplicitSelection}",
            workspaceName,
            capabilityType,
            selection.ModelConfigurationId,
            selection.ModelConfigurationId.HasValue);

        var resolved = await _runtimeConfigurationResolver.ResolveAsync(
            workspaceName,
            capabilityType,
            selection,
            cancellationToken);
        _runtimeConfigurationResolver.EnsureReady(resolved);
        var workspace = resolved.Workspace;
        var configuration = resolved.ToRequestModelConfiguration();
        if (capabilityType is AICapabilityType.ChatCompletion or AICapabilityType.VisionAnalysis)
        {
            await ApplySendTimeApiModeAsync(
                workspace,
                configuration,
                selection.RequiresToolCalling,
                requestedApiMode,
                cancellationToken);
        }

        var profile = AiProviderProfiles.Find(_profiles, workspace.Provider);
        var provider = profile == null
            ? _providers.FirstOrDefault(item => item.ProviderType == workspace.Provider)
            : _providers.FirstOrDefault(item => item.CapabilityKind == profile.CapabilityKind);
        if (provider == null)
        {
            _logger.LogDebug(
                "AI provider not registered. WorkspaceId={WorkspaceId}, WorkspaceName={WorkspaceName}, Provider={Provider}",
                workspace.Id,
                workspace.Name,
                workspace.Provider);
            throw new BusinessException(AIErrorCodes.ProviderNotSupported)
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
            throw new BusinessException(AIErrorCodes.CapabilityNotSupported)
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

        return (workspace, configuration, provider, resolved);
    }

    private async Task ApplySendTimeApiModeAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        bool requiresTools,
        OpenAIApiMode? requestedApiMode,
        CancellationToken cancellationToken)
    {
        var profile = AiProviderProfiles.Find(_profiles, workspace.Provider);
        var endpoints = await _endpointLookup.GetSupportedEndpointsAsync(
            workspace.Provider,
            configuration.ModelId,
            cancellationToken);
        var mode = OpenAIApiModePolicy.Select(
            profile?.SupportsApiMode(OpenAIApiMode.Responses) == true,
            endpoints,
            requiresTools,
            requestedApiMode);
        if (configuration.OpenAIApiMode == mode)
        {
            return;
        }

        configuration.UpdateConfiguration(
            configuration.ModelId,
            configuration.ApiEndpoint,
            configuration.ApiKey,
            configuration.Priority,
            mode,
            configuration.InputPrice,
            configuration.OutputPrice,
            configuration.Dimensions,
            configuration.DisplayName,
            configuration.IsUserSelectable,
            configuration.Description,
            configuration.MaxContextTokens,
            configuration.InputPriceUnit,
            configuration.OutputPriceUnit);
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
            AICapabilityType.WebSearch => SufiAIFeatures.WebSearch,
            AICapabilityType.WebFetch => SufiAIFeatures.WebFetch,
            _ => SufiAIFeatures.Enable
        };

        if (!await _featureChecker.IsEnabledAsync(featureName))
        {
            throw new BusinessException($"Feature is disabled: {featureName}");
        }
    }

    private Task LogUsageAsync(
        WorkspaceRuntimeConfiguration resolved,
        AICapabilityType capabilityType,
        int? inputTokens,
        int? outputTokens,
        int? totalTokens,
        string? usageUnavailableReason,
        long latencyMs,
        bool isSuccess,
        string? errorMessage = null,
        string? modelId = null,
        decimal? audioSeconds = null,
        int? characterCount = null,
        int? imageCount = null,
        CancellationToken cancellationToken = default)
    {
        return _usageRecorder.RecordAsync(
            new AIUsageRecord
            {
                Configuration = resolved,
                CapabilityType = capabilityType,
                ModelId = modelId,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = totalTokens,
                AudioSeconds = audioSeconds,
                CharacterCount = characterCount,
                ImageCount = imageCount,
                UsageUnavailableReason = usageUnavailableReason,
                LatencyMs = latencyMs,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            },
            cancellationToken);
    }

    private static string FormatLoggedError(Exception exception)
    {
        if (exception is BusinessException businessException
            && !string.IsNullOrWhiteSpace(businessException.Code))
        {
            return businessException.Code;
        }

        var message = exception.Message;
        if (string.IsNullOrWhiteSpace(message)
            || message.StartsWith("Exception of type '", StringComparison.Ordinal))
        {
            return exception.GetType().Name;
        }

        return message;
    }
}
