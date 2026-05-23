using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Identity;

public class GetIdentityUsersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
