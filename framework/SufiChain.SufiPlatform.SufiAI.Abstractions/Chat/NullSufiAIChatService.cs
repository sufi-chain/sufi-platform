using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Null fallback used when no AI provider module is installed.
/// Reports unavailability and throws <see cref="BusinessException"/> with
/// <see cref="SufiAIErrorCodes.ProviderNotAvailable"/> when invoked.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIChatService))]
public class NullSufiAIChatService : ISufiAIChatService, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public virtual Task<SufiAIChatResponse> CompleteAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAIErrorCodes.ProviderNotAvailable);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<SufiAIChatStreamChunk> StreamAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAIErrorCodes.ProviderNotAvailable);
    }
}
