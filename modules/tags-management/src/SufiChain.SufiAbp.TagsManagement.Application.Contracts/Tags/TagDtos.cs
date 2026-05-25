using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public class TagDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string? Color { get; set; }
}

public class UpdateTagDto
{
    public string Name { get; set; } = string.Empty;
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

