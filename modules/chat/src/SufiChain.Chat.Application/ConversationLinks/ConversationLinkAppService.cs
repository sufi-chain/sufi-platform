using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.ConversationLinks;

[Authorize(ChatPermissions.Links.Default)]
public class ConversationLinkAppService : ChatAppService, IConversationLinkAppService
{
    protected IConversationLinkRepository ConversationLinkRepository { get; }
    protected IChatSessionRepository SessionRepository { get; }
    protected ConversationLinkManager ConversationLinkManager { get; }
    protected ChatApplicationMapper Mapper { get; }

    public ConversationLinkAppService(
        IConversationLinkRepository conversationLinkRepository,
        IChatSessionRepository sessionRepository,
        ConversationLinkManager conversationLinkManager,
        ChatApplicationMapper mapper)
    {
        ConversationLinkRepository = conversationLinkRepository;
        SessionRepository = sessionRepository;
        ConversationLinkManager = conversationLinkManager;
        Mapper = mapper;
    }

    [Authorize(ChatPermissions.Links.Manage)]
    public virtual async Task<ConversationLinkDto> CreateAsync(CreateConversationLinkInput input)
    {
        var session = await SessionRepository.GetAsync(input.SessionId);
        var link = await ConversationLinkManager.CreateAsync(
            session,
            input.LinkedEntityType,
            input.LinkedEntityId,
            input.LinkRole,
            input.MetadataJson);

        return Mapper.ToDto(link);
    }

    public virtual async Task<List<ConversationLinkDto>> GetBySessionAsync(Guid sessionId)
    {
        var links = await ConversationLinkRepository.GetListBySessionAsync(sessionId);
        return links.Select(Mapper.ToDto).ToList();
    }

    public virtual async Task<List<ConversationLinkDto>> GetByEntityAsync(string linkedEntityType, Guid linkedEntityId)
    {
        var links = await ConversationLinkRepository.GetListByEntityAsync(linkedEntityType, linkedEntityId.ToString("D"));
        return links.Select(Mapper.ToDto).ToList();
    }

    [Authorize(ChatPermissions.Links.Manage)]
    public virtual async Task DeleteAsync(Guid linkId)
    {
        await ConversationLinkRepository.DeleteAsync(linkId);
    }
}
