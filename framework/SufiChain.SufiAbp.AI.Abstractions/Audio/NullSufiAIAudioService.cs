using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Null fallback used when no AI provider module is installed.
/// </summary>
[Dependency(TryRegister = true)]
[ExposeServices(typeof(ISufiAIAudioService))]
public class NullSufiAIAudioService : ISufiAIAudioService, ITransientDependency
{
    /// <inheritdoc />
    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public virtual Task<SufiAITranscriptionResponse> TranscribeAsync(
        SufiAITranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new BusinessException(SufiAIErrorCodes.ProviderNotAvailable);
    }
}
