using System;
using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Editions;

public class EditionDto : ExtensibleEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class EditionCreateDto
{
    [Required]
    [StringLength(EditionConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(EditionConsts.MaxDisplayNameLength)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(EditionConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class EditionUpdateDto : IHasConcurrencyStamp
{
    [Required]
    [StringLength(EditionConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(EditionConsts.MaxDisplayNameLength)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(EditionConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class GetEditionsInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
