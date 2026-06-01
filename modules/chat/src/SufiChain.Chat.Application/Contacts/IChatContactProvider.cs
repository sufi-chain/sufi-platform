using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Contacts;

public interface IChatContactProvider
{
    Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input);
}

public class NullChatContactProvider : IChatContactProvider, ITransientDependency
{
    public virtual Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input)
    {
        return Task.FromResult(new PagedResultDto<ChatContactDto>(
            0,
            new List<ChatContactDto>()));
    }
}
