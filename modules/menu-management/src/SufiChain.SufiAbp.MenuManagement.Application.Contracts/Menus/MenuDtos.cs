using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

public class MenuDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string ContextType { get; set; } = string.Empty;
    public Guid? ContextId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class MenuListDto : EntityDto<Guid>
{
    public string ContextType { get; set; } = string.Empty;
    public Guid? ContextId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateMenuDto
{
    public string ContextType { get; set; } = string.Empty;
    public Guid? ContextId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateMenuDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GetMenusInput : PagedAndSortedResultRequestDto
{
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}
