using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Supports;

public class ConfigurableAiService : ISufiAIChatService, ISingletonDependency
{
    public string ResponseContent { get; set; } = "Suggested operator reply";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<SufiAIChatResponse> CompleteAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SufiAIChatResponse
        {
            Content = ResponseContent,
            ModelId = "test-model",
            Usage = new SufiAITokenUsage
            {
                InputTokens = 5,
                OutputTokens = 10,
                TotalTokens = 15
            }
        });
    }

    public IAsyncEnumerable<SufiAIChatStreamChunk> StreamAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}
