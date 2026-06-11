using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.AI;

public interface ISufiAbpAIChatAppService : IApplicationService
{
    Task<SufiAbpAIChatResponseDto> SendMessageAsync(SufiAbpAISendChatMessageInput input);

    IAsyncEnumerable<SufiAbpAIChatResponseDto> StreamMessageAsync(SufiAbpAISendChatMessageInput input);
}
