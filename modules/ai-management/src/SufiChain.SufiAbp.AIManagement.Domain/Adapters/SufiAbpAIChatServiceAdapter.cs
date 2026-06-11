using System.Runtime.CompilerServices;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp.DependencyInjection;
using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIChatService))]
public class SufiAbpAIChatServiceAdapter : ISufiAbpAIChatService, ITransientDependency
{
    protected IAIService AIService { get; }

    public SufiAbpAIChatServiceAdapter(IAIService aiService)
    {
        AIService = aiService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public virtual async Task<SufiAbpAIChatResponse> CompleteAsync(
        SufiAbpAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await AIService.SendChatMessageAsync(MapRequest(request, stream: false), cancellationToken);
        return MapResponse(response);
    }

    public virtual async IAsyncEnumerable<SufiAbpAIChatStreamChunk> StreamAsync(
        SufiAbpAIChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in AIService.StreamChatMessageAsync(MapRequest(request, stream: true), cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            yield return new SufiAbpAIChatStreamChunk
            {
                Content = chunk.Content,
                ModelId = chunk.ModelId,
                Usage = chunk.IsUsageChunk ? MapUsage(chunk) : null,
                IsFinal = chunk.IsUsageChunk || !string.IsNullOrWhiteSpace(chunk.FinishReason),
                FinishReason = chunk.FinishReason
            };
        }
    }

    protected virtual ChatCompletionRequest MapRequest(SufiAbpAIChatRequest request, bool stream)
    {
        return new ChatCompletionRequest
        {
            WorkspaceName = request.WorkspaceName,
            SystemPrompt = request.SystemPrompt,
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            Stream = stream,
            Messages = request.Messages.Select(message => new ChatMessage
            {
                Role = message.Role,
                Content = message.Content
            }).ToList()
        };
    }

    protected virtual SufiAbpAIChatResponse MapResponse(ChatCompletionResponse response)
    {
        return new SufiAbpAIChatResponse
        {
            Content = response.Content,
            ModelId = response.ModelId,
            Usage = MapUsage(response),
            FinishReason = response.FinishReason
        };
    }

    protected virtual SufiAbpAITokenUsage MapUsage(ChatCompletionResponse response)
    {
        return new SufiAbpAITokenUsage
        {
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
            TotalTokens = response.TotalTokens,
            UnavailableReason = response.UsageUnavailableReason
        };
    }
}
