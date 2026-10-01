using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

public class EmptyModelEndpointLookup : IModelEndpointLookup, ITransientDependency
{
    public Task<IReadOnlyList<string>> GetSupportedEndpointsAsync(
        AIProviderType provider,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }
}
