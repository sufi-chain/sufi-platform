using Volo.Abp.Application.Services;

namespace SufiChain.Chat.ConversationLinks;

public interface IConversationLinkAppService : IApplicationService
{
    Task<ConversationLinkDto> CreateAsync(CreateConversationLinkInput input);

    Task<List<ConversationLinkDto>> GetBySessionAsync(Guid sessionId);

    Task<List<ConversationLinkDto>> GetByEntityAsync(string linkedEntityType, Guid linkedEntityId);

    Task DeleteAsync(Guid linkId);
}
