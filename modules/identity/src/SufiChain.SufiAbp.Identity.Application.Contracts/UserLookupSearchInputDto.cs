using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity;

public class UserLookupSearchInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
