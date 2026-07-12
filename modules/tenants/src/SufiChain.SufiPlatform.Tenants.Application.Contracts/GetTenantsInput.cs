using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Tenants;

public class GetTenantsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
