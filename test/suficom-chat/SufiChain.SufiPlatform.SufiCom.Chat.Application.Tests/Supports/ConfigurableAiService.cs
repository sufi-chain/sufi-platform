using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Supports;

public class ConfigurableAiService : ISufiAIChatService, ISingletonDependency
{
    public string ResponseContent { get; set; } = "Suggested operator reply";

    /// <summary>9.4 (sufi-chain#340): tests set this to prove a slow model is not cut off.</summary>
    public TimeSpan CompleteDelay { get; set; }

    /// <summary>Runs after <see cref="CompleteDelay"/> and before the reply, on the caller token.</summary>
    public Func<CancellationToken, Task>? BeforeComplete { get; set; }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public async Task<SufiAIChatResponse> CompleteAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (CompleteDelay > TimeSpan.Zero)
        {
            await Task.Delay(CompleteDelay, cancellationToken);
        }

        if (BeforeComplete != null)
        {
            await BeforeComplete(cancellationToken);
        }

        return new SufiAIChatResponse
        {
            Content = ResponseContent,
            ModelId = "test-model",
            Usage = new SufiAITokenUsage
            {
                InputTokens = 5,
                OutputTokens = 10,
                TotalTokens = 15
            }
        };
    }

    public IAsyncEnumerable<SufiAIChatStreamChunk> StreamAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}
