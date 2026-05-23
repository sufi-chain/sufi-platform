using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.Chat;

public interface IAIChatAppService : IApplicationService
{
    Task<ChatResponseDto> SendMessageAsync(SendChatMessageInput input);
}
