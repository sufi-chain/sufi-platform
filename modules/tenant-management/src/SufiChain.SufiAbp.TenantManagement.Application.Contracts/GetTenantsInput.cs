using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.TenantManagement;

public class GetTenantsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
