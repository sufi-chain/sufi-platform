using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Application service for accessing AI kernels by workspace.
/// Provides a layer between HttpApi and Domain services.
/// </summary>
public interface IAIKernelAppService : IApplicationService
{
    /// <summary>
    /// Gets a Semantic Kernel instance for the specified workspace.
    /// Returns an opaque object that can be cast to Microsoft.SemanticKernel.Kernel.
    /// </summary>
    Task<object> GetKernelAsync(string workspaceName, CancellationToken cancellationToken = default);
}
