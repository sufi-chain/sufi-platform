using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AIManagement.MCP.Abstractions;

public interface IMCPKernelToolRegistrar
{
    Task RegisterToolsAsync(
        Kernel kernel,
        string workspaceName,
        WorkspaceContext context,
        CancellationToken cancellationToken = default);
}
