using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiPlatform.Tenants;

public abstract class TenantCreateOrUpdateDtoBase : ExtensibleObject
{
    [Required]
    [MaxLength(64)]
    public virtual string Name { get; set; } = null!;

    public virtual Guid? EditionId { get; set; }

    public virtual Guid? OwnerUserId { get; set; }

    [MaxLength(TenantConsts.MaxSubdomainLength)]
    public virtual string? PrimarySubdomain { get; set; }

    public virtual List<TenantDomainInputDto> Domains { get; set; } = [];
}

public class TenantDomainInputDto
{
    [Required]
    [MaxLength(TenantConsts.MaxDomainHostLength)]
    public string Host { get; set; } = null!;

    public TenantDomainType Type { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }
}
