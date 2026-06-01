using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Sessions;

public interface IChatSessionAppService : IApplicationService
{
    Task<ChatSessionDto> CreateAsync(CreateChatSessionInput input);

    Task<ChatSessionDto> GetAsync(Guid id);

    Task<PagedResultDto<ChatSessionListDto>> GetListAsync(GetChatSessionListInput input);

    Task CloseAsync(Guid id, CloseChatSessionInput? input = null);

    Task AddParticipantAsync(Guid sessionId, AddChatParticipantInput input);

    Task RemoveParticipantAsync(Guid sessionId, Guid participantId);

    Task<PagedResultDto<ChatSessionListDto>> GetMySessionsAsync(GetMyChatSessionsInput input);

    Task<ChatSessionDto> GetOrCreateDirectSessionAsync(GetOrCreateDirectSessionInput input);

    Task<ChatSessionDto> CreateGroupSessionAsync(CreateGroupChatSessionInput input);

    Task<ChatSessionDto> JoinGroupSessionAsync(Guid sessionId);

    Task LeaveGroupSessionAsync(Guid sessionId);
}
