using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Permissions;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.AIManagement.AI;

[RequiresFeature(SufiAbpAIFeatures.Enable)]
[Authorize(AIManagementPermissions.AI.Chat)]
public class SufiAbpAIChatAppService : SufiAbpApplicationService, ISufiAbpAIChatAppService
{
    protected ISufiAbpAIChatService ChatService { get; }

    public SufiAbpAIChatAppService(ISufiAbpAIChatService chatService)
    {
        ChatService = chatService;
    }

    [RequiresFeature(SufiAbpAIFeatures.Chat)]
    public virtual async Task<SufiAbpAIChatResponseDto> SendMessageAsync(SufiAbpAISendChatMessageInput input)
    {
        var request = MapRequest(input);
        var response = await ChatService.CompleteAsync(request);

        return new SufiAbpAIChatResponseDto
        {
            Message = response.Content,
            Model = response.ModelId,
            TokensUsed = response.Usage.TotalTokens,
            InputTokens = response.Usage.InputTokens,
            OutputTokens = response.Usage.OutputTokens
        };
    }

    [RequiresFeature(SufiAbpAIFeatures.Chat)]
    public virtual async IAsyncEnumerable<SufiAbpAIChatResponseDto> StreamMessageAsync(SufiAbpAISendChatMessageInput input)
    {
        await foreach (var chunk in ChatService.StreamAsync(MapRequest(input)))
        {
            yield return new SufiAbpAIChatResponseDto
            {
                Message = chunk.Content,
                Model = chunk.ModelId,
                TokensUsed = chunk.Usage?.TotalTokens,
                InputTokens = chunk.Usage?.InputTokens,
                OutputTokens = chunk.Usage?.OutputTokens
            };
        }
    }

    protected virtual SufiAbpAIChatRequest MapRequest(SufiAbpAISendChatMessageInput input)
    {
        var request = new SufiAbpAIChatRequest
        {
            WorkspaceName = input.WorkspaceName,
            Temperature = input.Temperature,
            MaxTokens = input.MaxTokens,
            Messages = input.ConversationHistory.Select(message => new SufiAbpAIChatMessage
            {
                Role = message.Role,
                Content = message.Content
            }).ToList()
        };

        request.Messages.Add(new SufiAbpAIChatMessage
        {
            Role = SufiAbpAIChatRoles.User,
            Content = input.Message
        });

        return request;
    }
}
