using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Users;

public class UserLookupSearchInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
