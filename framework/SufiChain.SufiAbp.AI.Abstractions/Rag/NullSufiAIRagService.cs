using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no RAG provider module is installed.
/// Search returns no results; indexing throws so callers can surface the misconfiguration.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIRagService))]
public class NullSufiAIRagService : ISufiAIRagService, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public virtual Task<SufiAIRagSearchResult> SearchAsync(
        SufiAIRagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SufiAIRagSearchResult());
    }

    /// <inheritdoc />
    public virtual Task IndexAsync(
        SufiAIRagIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAIErrorCodes.ProviderNotAvailable);
    }

    /// <inheritdoc />
    public virtual Task<SufiAIIndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SufiAIIndexingStatus
        {
            SourceName = sourceName
        });
    }
}
