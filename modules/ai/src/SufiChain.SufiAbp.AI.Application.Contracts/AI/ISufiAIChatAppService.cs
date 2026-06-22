using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.AI;

public interface ISufiAIChatAppService : IApplicationService
{
    Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input);

    Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input);

    IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input);
}
