using System;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.TenantManagement;

public class TenantDto : ExtensibleEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = null!;

    public string ConcurrencyStamp { get; set; } = null!;
}
