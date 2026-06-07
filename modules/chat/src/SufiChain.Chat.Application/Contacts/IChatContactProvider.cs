using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Contacts;

public interface IChatContactProvider
{
    Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input);
}

public class NullChatContactProvider : IChatContactProvider
{
    public virtual Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input)
    {
        return Task.FromResult(new PagedResultDto<ChatContactDto>(
            0,
            new List<ChatContactDto>()));
    }
}
