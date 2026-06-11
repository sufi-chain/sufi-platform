using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Platform-level chat completion service for product modules.
/// This is a stable public contract: product modules (Chat, HelpDesk, Calendar, ...)
/// depend on this interface only; a provider module (e.g. AIManagement) supplies the
/// implementation by replacing the Null default. Provider SDK types
/// (Semantic Kernel, Microsoft.Extensions.AI) are never exposed here.
/// </summary>
public interface ISufiAbpAIChatService
{
    /// <summary>
    /// Whether a real AI provider implementation is installed and usable.
    /// Returns <c>false</c> for the Null fallback so modules can degrade gracefully.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a non-streaming chat completion.
    /// </summary>
    Task<SufiAbpAIChatResponse> CompleteAsync(
        SufiAbpAIChatRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a streaming chat completion. The final chunk carries token usage
    /// when the provider reports it; record usage once per completed stream.
    /// </summary>
    IAsyncEnumerable<SufiAbpAIChatStreamChunk> StreamAsync(
        SufiAbpAIChatRequest request,
        CancellationToken cancellationToken = default);
}
