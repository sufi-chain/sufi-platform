using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity;

public class UserLookupSearchInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
