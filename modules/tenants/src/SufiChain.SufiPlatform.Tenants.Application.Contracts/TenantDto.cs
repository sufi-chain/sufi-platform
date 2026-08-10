using System;
using System.Collections.Generic;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDto : ExtensibleEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = null!;

    public Guid? EditionId { get; set; }

    public Guid? OwnerUserId { get; set; }

    public string? DatabaseName { get; set; }

    public string? PrimarySubdomain { get; set; }

    public List<TenantDomainDto> Domains { get; set; } = [];

    public string ConcurrencyStamp { get; set; } = null!;
}

public class TenantDomainDto
{
    public Guid Id { get; set; }

    public string Host { get; set; } = null!;

    public TenantDomainType Type { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }
}
