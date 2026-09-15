using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIToolChatExecutor : ITransientDependency
{
    Task<AIToolChatExecutionResult> ExecuteAsync(
        AIToolChatExecutionRequest request,
        CancellationToken cancellationToken = default);

    Task<AIPreparedKernel> PrepareKernelAsync(
        string workspaceName,
        bool requiresToolCalling = false,
        CancellationToken cancellationToken = default);

    Task<AIPreparedKernel> PrepareKernelAsync(
        WorkspaceRuntimeConfiguration configuration,
        bool requiresToolCalling = false,
        CancellationToken cancellationToken = default);
}

public sealed class AIToolChatExecutionRequest
{
    public required WorkspaceRuntimeConfiguration Configuration { get; init; }

    public required ChatHistory History { get; init; }

    public PromptExecutionSettings? ExecutionSettings { get; init; }

    public bool RequiresToolCalling { get; init; }

    public Func<Kernel, CancellationToken, Task>? ConfigureKernelAsync { get; init; }

    public Kernel? Kernel { get; init; }
}

public sealed class AIToolChatExecutionResult
{
    public required ChatMessageContent Response { get; init; }

    public required SufiAITokenUsage Usage { get; init; }

    public required long LatencyMs { get; init; }

    public required WorkspaceRuntimeConfiguration Configuration { get; init; }
}

public sealed class AIPreparedKernel
{
    public required Kernel Kernel { get; init; }

    public required WorkspaceRuntimeConfiguration Configuration { get; init; }
}
