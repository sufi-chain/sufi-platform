using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Platform-level text-to-image service for product modules. Returns bytes only;
/// storing the image (for example in File Manager) is the caller's responsibility.
/// </summary>
public interface ISufiAIImageService
{
    /// <summary>
    /// Whether the workspace has a configured model and a provider that supports image generation.
    /// Returns <c>false</c> for the Null fallback.
    /// </summary>
    Task<bool> IsAvailableAsync(string workspaceName, CancellationToken cancellationToken = default);

    Task<SufiAIImageGenerationResponse> GenerateAsync(
        SufiAIImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}
