using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AI.MCP.Abstractions;

public interface IMCPKernelToolRegistrar
{
    Task RegisterToolsAsync(
        Kernel kernel,
        string workspaceName,
        WorkspaceContext context,
        CancellationToken cancellationToken = default);
}
