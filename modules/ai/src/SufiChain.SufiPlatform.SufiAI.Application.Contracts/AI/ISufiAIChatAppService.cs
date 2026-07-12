using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI;

public interface ISufiAIChatAppService : IApplicationService
{
    Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input);

    Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input);

    IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input);
}
