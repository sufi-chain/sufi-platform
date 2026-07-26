using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Resolves RAG embedder settings from multimodal <see cref="AIModelConfiguration"/> rows.
/// </summary>
public interface IWorkspaceEmbedderResolver
{
    Task<EmbedderConfiguration> ResolveAsync(Workspace workspace, CancellationToken cancellationToken = default);
}
