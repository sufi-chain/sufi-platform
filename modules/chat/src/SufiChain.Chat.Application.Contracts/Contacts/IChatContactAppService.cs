using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.Chat.Contacts;

public interface IChatContactAppService : IApplicationService
{
    Task<PagedResultDto<ChatContactDto>> SearchAsync(SearchChatContactsInput input);
}
