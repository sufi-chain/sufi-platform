using System;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDto : ExtensibleEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = null!;

    public Guid? EditionId { get; set; }

    public Guid? OwnerUserId { get; set; }

    public string ConcurrencyStamp { get; set; } = null!;
}
