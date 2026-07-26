using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantUpdateDto : TenantCreateOrUpdateDtoBase, IHasConcurrencyStamp
{
    public Guid? EditionId { get; set; }

    public Guid? OwnerUserId { get; set; }

    public string ConcurrencyStamp { get; set; } = null!;
}
