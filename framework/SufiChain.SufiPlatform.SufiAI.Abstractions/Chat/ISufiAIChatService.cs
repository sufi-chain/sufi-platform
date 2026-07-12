using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Platform-level chat completion service for product modules.
/// This is a stable public contract: product modules (Chat, HelpDesk, Calendar, ...)
/// depend on this interface only; a provider module (e.g. AI) supplies the
/// implementation by replacing the Null default. Provider SDK types
/// (Semantic Kernel, Microsoft.Extensions.AI) are never exposed here.
/// </summary>
public interface ISufiAIChatService
{
    /// <summary>
    /// Whether a real AI provider implementation is installed and usable.
    /// Returns <c>false</c> for the Null fallback so modules can degrade gracefully.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a non-streaming chat completion.
    /// </summary>
    Task<SufiAIChatResponse> CompleteAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a streaming chat completion. The final chunk carries token usage
    /// when the provider reports it; record usage once per completed stream.
    /// </summary>
    IAsyncEnumerable<SufiAIChatStreamChunk> StreamAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default);
}
