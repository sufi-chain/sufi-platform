using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantUpdateDto : TenantCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = null!;
}
