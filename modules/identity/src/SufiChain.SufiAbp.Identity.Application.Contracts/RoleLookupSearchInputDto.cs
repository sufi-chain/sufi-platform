using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity;

public class RoleLookupSearchInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
