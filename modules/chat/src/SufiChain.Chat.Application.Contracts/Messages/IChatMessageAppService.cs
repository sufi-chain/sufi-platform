using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Messages;

public interface IChatMessageAppService : IApplicationService
{
    Task<ChatMessageDto> SendAsync(SendChatMessageInput input);

    Task<PagedResultDto<ChatMessageDto>> GetListAsync(GetChatMessageListInput input);

    Task DeleteAsync(Guid messageId);
}
