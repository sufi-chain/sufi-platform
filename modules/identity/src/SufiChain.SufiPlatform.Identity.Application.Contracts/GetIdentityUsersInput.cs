using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Identity;

public class GetIdentityUsersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
