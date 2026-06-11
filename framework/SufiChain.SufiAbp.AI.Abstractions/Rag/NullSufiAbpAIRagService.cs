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
[ExposeServices(typeof(ISufiAbpAIRagService))]
public class NullSufiAbpAIRagService : ISufiAbpAIRagService, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public virtual Task<SufiAbpAIRagSearchResult> SearchAsync(
        SufiAbpAIRagSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SufiAbpAIRagSearchResult());
    }

    /// <inheritdoc />
    public virtual Task IndexAsync(
        SufiAbpAIRagIndexRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAbpAIErrorCodes.ProviderNotAvailable);
    }

    /// <inheritdoc />
    public virtual Task<SufiAbpAIIndexingStatus> GetIndexingStatusAsync(
        string workspaceName,
        string sourceName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SufiAbpAIIndexingStatus
        {
            SourceName = sourceName
        });
    }
}
