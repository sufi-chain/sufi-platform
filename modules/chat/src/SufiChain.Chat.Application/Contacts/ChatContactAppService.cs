using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Contacts;

public class ChatContactAppService : ChatAppService, IChatContactAppService
{
    protected IChatContactProvider ContactProvider { get; }

    public ChatContactAppService(IChatContactProvider contactProvider)
    {
        ContactProvider = contactProvider;
    }

    public virtual Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input)
    {
        return ContactProvider.SearchAsync(input);
    }
}
