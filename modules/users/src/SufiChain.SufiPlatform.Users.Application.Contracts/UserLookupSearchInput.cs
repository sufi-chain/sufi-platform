using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Users;

public class UserLookupSearchInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
