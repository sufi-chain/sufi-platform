using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity;

public class RoleLookupSearchInputDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
