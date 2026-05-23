using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.TenantManagement;

public class TenantUpdateDto : TenantCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = null!;
}
