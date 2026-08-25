using System.Runtime.CompilerServices;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.DependencyInjection;
using DependencyAttribute = Volo.Abp.DependencyInjection.DependencyAttribute;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIChatService))]
public class SufiAIChatServiceAdapter : ISufiAIChatService, ITransientDependency
{
    protected IAIService AIService { get; }

    public SufiAIChatServiceAdapter(IAIService aiService)
    {
        AIService = aiService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public virtual async Task<SufiAIChatResponse> CompleteAsync(
        SufiAIChatRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await AIService.SendChatMessageAsync(MapRequest(request, stream: false), cancellationToken);
        return MapResponse(response);
    }

    public virtual async IAsyncEnumerable<SufiAIChatStreamChunk> StreamAsync(
        SufiAIChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var chunk in AIService.StreamChatMessageAsync(MapRequest(request, stream: true), cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            yield return new SufiAIChatStreamChunk
            {
                Content = chunk.Content,
                ModelId = chunk.ModelId,
                Usage = chunk.IsUsageChunk ? MapUsage(chunk) : null,
                IsFinal = chunk.IsUsageChunk || !string.IsNullOrWhiteSpace(chunk.FinishReason),
                FinishReason = chunk.FinishReason
            };
        }
    }

    protected virtual ChatCompletionRequest MapRequest(SufiAIChatRequest request, bool stream)
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
                Content = message.Content,
                MultiModalContent = message.ContentParts.Count == 0
                    ? null
                    : message.ContentParts.Select(part => part.Type switch
                    {
                        "text" => new MessageContent { Type = "text", Text = part.Text ?? message.Content },
                        "image" => new MessageContent
                        {
                            Type = "image_url",
                            ImageUrl = new ImageContent { Url = part.DataUrl ?? string.Empty }
                        },
                        _ => null
                    }).Where(part => part != null).Cast<MessageContent>().ToList()
            }).ToList()
        };
    }

    protected virtual SufiAIChatResponse MapResponse(ChatCompletionResponse response)
    {
        return new SufiAIChatResponse
        {
            Content = response.Content,
            ModelId = response.ModelId,
            Usage = MapUsage(response),
            FinishReason = response.FinishReason
        };
    }

    protected virtual SufiAITokenUsage MapUsage(ChatCompletionResponse response)
    {
        return new SufiAITokenUsage
        {
            InputTokens = response.InputTokens,
            OutputTokens = response.OutputTokens,
            TotalTokens = response.TotalTokens,
            UnavailableReason = response.UsageUnavailableReason
        };
    }
}
