using Microsoft.SemanticKernel;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

public interface IMCPKernelToolRegistrar
{
    Task RegisterToolsAsync(
        Kernel kernel,
        WorkspaceContext context,
        IReadOnlyList<string> allowedToolNames,
        CancellationToken cancellationToken = default);
}
