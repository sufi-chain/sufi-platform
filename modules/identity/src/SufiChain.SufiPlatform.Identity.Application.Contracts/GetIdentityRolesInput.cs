using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity;

public class GetIdentityRolesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
