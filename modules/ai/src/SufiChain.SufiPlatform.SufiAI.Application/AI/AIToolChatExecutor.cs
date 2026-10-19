using System.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public class AIToolChatExecutor : IAIToolChatExecutor, ITransientDependency
{
    protected IWorkspaceRuntimeConfigurationResolver RuntimeConfigurationResolver { get; }
    protected WorkspaceSyncService WorkspaceSyncService { get; }
    protected IWorkspaceGuardrailService WorkspaceGuardrailService { get; }
    protected IAIUsageRecorder UsageRecorder { get; }
    protected ILogger<AIToolChatExecutor> Logger { get; }
    protected AIToolLoopOptions ToolLoopOptions { get; }
    protected IStringLocalizer<AIResource>? Localizer { get; }

    public AIToolChatExecutor(
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        WorkspaceSyncService workspaceSyncService,
        IWorkspaceGuardrailService workspaceGuardrailService,
        IAIUsageRecorder usageRecorder,
        ILogger<AIToolChatExecutor>? logger = null,
        IOptions<AIToolLoopOptions>? toolLoopOptions = null,
        IStringLocalizer<AIResource>? localizer = null)
    {
        ToolLoopOptions = toolLoopOptions?.Value ?? new AIToolLoopOptions();
        Localizer = localizer;
        RuntimeConfigurationResolver = runtimeConfigurationResolver;
        WorkspaceSyncService = workspaceSyncService;
        WorkspaceGuardrailService = workspaceGuardrailService;
        UsageRecorder = usageRecorder;
        Logger = logger ?? NullLogger<AIToolChatExecutor>.Instance;
    }

    public virtual async Task<AIPreparedKernel> PrepareKernelAsync(
        string workspaceName,
        bool requiresToolCalling = false,
        CancellationToken cancellationToken = default)
    {
        var configuration = await RuntimeConfigurationResolver.ResolveAsync(
            workspaceName,
            AICapabilityType.ChatCompletion,
            cancellationToken);
        return await PrepareKernelAsync(configuration, requiresToolCalling, cancellationToken);
    }

    public virtual async Task<AIPreparedKernel> PrepareKernelAsync(
        WorkspaceRuntimeConfiguration configuration,
        bool requiresToolCalling = false,
        CancellationToken cancellationToken = default)
    {
        await WorkspaceGuardrailService.EnsureCanExecuteAsync(configuration.Workspace.Id, cancellationToken);
        if (requiresToolCalling && configuration.OpenAIApiMode != OpenAIApiMode.ChatCompletions)
        {
            configuration = configuration.WithOpenAIApiMode(OpenAIApiMode.ChatCompletions);
        }

        RuntimeConfigurationResolver.EnsureReady(configuration, requiresToolCalling);
        var kernel = await WorkspaceSyncService.CreateRequestKernelAsync(configuration, cancellationToken);
        return new AIPreparedKernel
        {
            Kernel = kernel,
            Configuration = configuration
        };
    }

    public virtual async Task<AIToolChatExecutionResult> ExecuteAsync(
        AIToolChatExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var kernel = request.Kernel;
        if (kernel == null)
        {
            var prepared = await PrepareKernelAsync(
                request.Configuration,
                request.RequiresToolCalling,
                cancellationToken);
            kernel = prepared.Kernel;
        }

        if (request.ConfigureKernelAsync != null)
        {
            await request.ConfigureKernelAsync(kernel, cancellationToken);
        }

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var executionSettings = request.ExecutionSettings;
        if (request.ResponseSchema != null)
        {
            var settings = (OpenAIPromptExecutionSettings)OpenAIPromptExecutionSettings
                .FromExecutionSettings(executionSettings).Clone();
            settings.ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                request.ResponseSchema.Name,
                BinaryData.FromString(request.ResponseSchema.SchemaJson),
                jsonSchemaIsStrict: true);
            executionSettings = settings;
        }
        // sufi-chain#340: bound the turn by tool rounds and wall-clock time, ending with a visible message.
        var maxDuration = TimeSpan.FromSeconds(Math.Max(1, ToolLoopOptions.MaxTurnSeconds));
        var loopLimit = new AIToolLoopLimitFilter(ToolLoopOptions.MaxToolRounds, maxDuration);
        kernel.AutoFunctionInvocationFilters.Add(loopLimit);
        using var turnTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        turnTimeout.CancelAfter(maxDuration);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Microsoft.SemanticKernel.ChatMessageContent response;
            try
            {
                response = request.BufferStreamingResponse
                    ? await AIStreamingChatResponseReader.ReadAsync(chatService.GetStreamingChatMessageContentsAsync(
                        request.History, executionSettings, kernel, turnTimeout.Token), turnTimeout.Token)
                    : await chatService.GetChatMessageContentAsync(
                        request.History, executionSettings, kernel, turnTimeout.Token);
            }
            catch (OperationCanceledException) when (turnTimeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                Logger.LogWarning(
                    "AI tool turn hit the time limit. MaxTurnSeconds={MaxTurnSeconds}, ElapsedMs={ElapsedMs}",
                    ToolLoopOptions.MaxTurnSeconds, stopwatch.ElapsedMilliseconds);
                return await LimitReachedAsync(request, stopwatch.ElapsedMilliseconds, AIToolLoopFinishReasons.TimeLimit, cancellationToken);
            }
            stopwatch.Stop();

            if (loopLimit.LimitReached)
            {
                Logger.LogWarning(
                    "AI tool turn hit the tool-round limit. MaxToolRounds={MaxToolRounds}, ElapsedMs={ElapsedMs}",
                    ToolLoopOptions.MaxToolRounds, stopwatch.ElapsedMilliseconds);
                return await LimitReachedAsync(request, stopwatch.ElapsedMilliseconds, AIToolLoopFinishReasons.ToolRoundLimit, cancellationToken);
            }

            var usage = SemanticKernelChatTokenUsageExtractor.Extract(response);
            await TryRecordAsync(
                request.Configuration,
                response.ModelId,
                usage,
                stopwatch.ElapsedMilliseconds,
                isSuccess: true,
                cancellationToken: cancellationToken);

            return new AIToolChatExecutionResult
            {
                Response = response,
                Usage = usage,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                Configuration = request.Configuration
            };
        }
        catch (Exception exception) when (exception is not BusinessException)
        {
            stopwatch.Stop();
            // Diagnose protocol pairing without recording prompts, arguments, results, or credentials.
            var pendingCalls = new HashSet<string>(StringComparer.Ordinal);
            var callCount = 0;
            var resultCount = 0;
            var unmatchedResults = 0;
            foreach (var message in request.History)
            {
                foreach (var call in message.Items.OfType<FunctionCallContent>())
                {
                    callCount++;
                    if (message.Role == AuthorRole.Assistant && !string.IsNullOrEmpty(call.Id))
                        pendingCalls.Add(call.Id);
                }
                foreach (var result in message.Items.OfType<FunctionResultContent>())
                {
                    resultCount++;
                    if (string.IsNullOrEmpty(result.CallId) || !pendingCalls.Remove(result.CallId))
                        unmatchedResults++;
                }
            }
            Logger.LogWarning(
                "AI tool turn failed. BufferedStream={BufferedStream}, ToolCalls={ToolCalls}, ToolResults={ToolResults}, UnmatchedResults={UnmatchedResults}, PendingCalls={PendingCalls}, ElapsedMs={ElapsedMs}, ExceptionType={ExceptionType}, CallerCancelled={CallerCancelled}",
                request.BufferStreamingResponse, callCount, resultCount, unmatchedResults, pendingCalls.Count,
                stopwatch.ElapsedMilliseconds, exception.GetType().Name, cancellationToken.IsCancellationRequested);
            await TryRecordAsync(
                request.Configuration,
                request.Configuration.ModelId,
                usage: new SufiAITokenUsage(),
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: exception.Message,
                cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// sufi-chain#340: the turn ends with a visible message instead of looping or hanging. The reply carries
    /// <paramref name="finishReason"/> (<see cref="AIToolLoopFinishReasons"/>) so the turn is not logged as a success.
    /// </summary>
    protected virtual async Task<AIToolChatExecutionResult> LimitReachedAsync(
        AIToolChatExecutionRequest request,
        long latencyMs,
        string finishReason,
        CancellationToken cancellationToken)
    {
        await TryRecordAsync(
            request.Configuration,
            request.Configuration.ModelId,
            new SufiAITokenUsage(),
            latencyMs,
            isSuccess: false,
            errorMessage: AIErrorCodes.ToolLoopLimitReached,
            cancellationToken);
        var text = Localizer?[AIErrorCodes.ToolLoopLimitReached].Value;
        if (string.IsNullOrWhiteSpace(text) || text == AIErrorCodes.ToolLoopLimitReached)
        {
            text = "This took too long, so I stopped. Any changes already shown are kept. Try a smaller request.";
        }

        return new AIToolChatExecutionResult
        {
            Response = new Microsoft.SemanticKernel.ChatMessageContent(
                AuthorRole.Assistant,
                text,
                modelId: request.Configuration.ModelId,
                metadata: new Dictionary<string, object?> { ["FinishReason"] = finishReason }),
            Usage = new SufiAITokenUsage(),
            LatencyMs = latencyMs,
            Configuration = request.Configuration
        };
    }

    /// <summary>Recording usage must never fail the turn (sufi-chain#340).</summary>
    protected virtual async Task TryRecordAsync(
        WorkspaceRuntimeConfiguration configuration,
        string? modelId,
        SufiAITokenUsage usage,
        long latencyMs,
        bool isSuccess,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await RecordAsync(configuration, modelId, usage, latencyMs, isSuccess, errorMessage, cancellationToken);
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "AI usage could not be recorded for this tool turn.");
        }
    }

    protected virtual Task RecordAsync(
        WorkspaceRuntimeConfiguration configuration,
        string? modelId,
        SufiAITokenUsage usage,
        long latencyMs,
        bool isSuccess,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        return UsageRecorder.RecordAsync(
            new AIUsageRecord
            {
                Configuration = configuration,
                CapabilityType = AICapabilityType.ChatCompletion,
                ModelId = modelId ?? configuration.ModelId,
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens,
                TotalTokens = usage.TotalTokens,
                UsageUnavailableReason = usage.HasUsage ? null : "ProviderDidNotReturnUsage",
                LatencyMs = latencyMs,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            },
            cancellationToken);
    }
}
