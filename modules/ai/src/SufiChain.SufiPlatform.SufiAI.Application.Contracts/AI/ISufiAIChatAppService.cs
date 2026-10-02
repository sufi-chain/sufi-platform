using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI;

public interface ISufiAIChatAppService : IApplicationService
{
    Task<SufiAIChatResponseDto> SendMessageAsync(
        SufiAISendChatMessageInput input,
        CancellationToken cancellationToken = default);

    Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input);

    IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input);
}
