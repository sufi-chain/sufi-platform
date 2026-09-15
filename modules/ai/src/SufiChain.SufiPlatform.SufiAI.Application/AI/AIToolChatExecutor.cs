using System.Diagnostics;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
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

    public AIToolChatExecutor(
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        WorkspaceSyncService workspaceSyncService,
        IWorkspaceGuardrailService workspaceGuardrailService,
        IAIUsageRecorder usageRecorder)
    {
        RuntimeConfigurationResolver = runtimeConfigurationResolver;
        WorkspaceSyncService = workspaceSyncService;
        WorkspaceGuardrailService = workspaceGuardrailService;
        UsageRecorder = usageRecorder;
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
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await chatService.GetChatMessageContentAsync(
                request.History,
                request.ExecutionSettings,
                kernel,
                cancellationToken);
            stopwatch.Stop();

            var usage = SemanticKernelChatTokenUsageExtractor.Extract(response);
            await RecordAsync(
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
            await RecordAsync(
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
