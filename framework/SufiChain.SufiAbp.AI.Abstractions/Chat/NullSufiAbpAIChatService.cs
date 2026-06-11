using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no AI provider module is installed.
/// Reports unavailability and throws <see cref="BusinessException"/> with
/// <see cref="SufiAbpAIErrorCodes.ProviderNotAvailable"/> when invoked.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAbpAIChatService))]
public class NullSufiAbpAIChatService : ISufiAbpAIChatService, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public virtual Task<SufiAbpAIChatResponse> CompleteAsync(
        SufiAbpAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAbpAIErrorCodes.ProviderNotAvailable);
    }

    /// <inheritdoc />
    public virtual IAsyncEnumerable<SufiAbpAIChatStreamChunk> StreamAsync(
        SufiAbpAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAbpAIErrorCodes.ProviderNotAvailable);
    }
}
