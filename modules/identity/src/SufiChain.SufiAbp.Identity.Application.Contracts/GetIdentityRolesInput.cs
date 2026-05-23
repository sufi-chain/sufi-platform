using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity;

public class GetIdentityRolesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
