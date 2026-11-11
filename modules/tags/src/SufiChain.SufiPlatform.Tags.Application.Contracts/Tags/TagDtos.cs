using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Tags.Tags;

public class TagDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class CreateTagDto
{
    [Required]
    [StringLength(TagConsts.MaxNameLength, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(TagScopeConsts.MaxScopeLength, MinimumLength = 1)]
    public string Scope { get; set; } = string.Empty;

    public string? Color { get; set; }
}

public class UpdateTagDto
{
    [Required]
    [StringLength(TagConsts.MaxNameLength, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(TagScopeConsts.MaxScopeLength, MinimumLength = 1)]
    public string Scope { get; set; } = string.Empty;

    public string? Color { get; set; }
}

public class AssignTagDto
{
    public Guid TagId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
}

public class EntityTagQueryInput
{
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
}

public class TagLinkDto : EntityDto<Guid>
{
    public Guid TagId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
}

public class SearchTagsInput
{
    public string? Scope { get; set; }

    public string? Filter { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 20;
}

