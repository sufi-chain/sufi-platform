using System;
using System.Threading.Tasks;
using SufiChain.Chat.Sessions;
using Volo.Abp.Domain.Services;

namespace SufiChain.Chat.ConversationLinks;

public class ConversationLinkManager : DomainService
{
    private readonly IConversationLinkRepository _conversationLinkRepository;

    public ConversationLinkManager(IConversationLinkRepository conversationLinkRepository)
    {
        _conversationLinkRepository = conversationLinkRepository;
    }

    public virtual async Task<ConversationLink> CreateAsync(
        ChatSession session,
        string linkedEntityType,
        string linkedEntityId,
        string? linkRole = null,
        string? metadataJson = null)
    {
        var link = new ConversationLink(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            session.Id,
            linkedEntityType,
            linkedEntityId,
            linkRole,
            metadataJson);

        await _conversationLinkRepository.InsertAsync(link, autoSave: true);

        return link;
    }

    public virtual async Task<ConversationLink> CreateAsync(
        ChatSession session,
        string linkedEntityType,
        Guid linkedEntityId,
        string? linkRole = null,
        string? metadataJson = null)
    {
        return await CreateAsync(
            session,
            linkedEntityType,
            linkedEntityId.ToString("D"),
            linkRole,
            metadataJson);
    }
}
