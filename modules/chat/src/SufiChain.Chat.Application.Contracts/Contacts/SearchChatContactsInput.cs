using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.Chat.Contacts;

public class SearchChatContactsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public bool OnlineOnly { get; set; }
}
