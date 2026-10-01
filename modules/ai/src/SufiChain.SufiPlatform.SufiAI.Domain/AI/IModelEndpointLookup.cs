using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Catalog endpoints published for a provider model. An empty list means Chat Completions only.
/// </summary>
public interface IModelEndpointLookup
{
    Task<IReadOnlyList<string>> GetSupportedEndpointsAsync(
        AIProviderType provider,
        string modelId,
        CancellationToken cancellationToken = default);
}
