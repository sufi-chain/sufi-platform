namespace SufiChain.SufiAbp.MenuManagement.Menus;

public static class MenuMappingExtensions
{
    public static MenuDto ToDto(this Menu menu) => new()
    {
        Id = menu.Id,
        TenantId = menu.TenantId,
        ContextType = menu.ContextType,
        ContextId = menu.ContextId,
        Name = menu.Name,
        DisplayName = menu.DisplayName,
        Description = menu.Description,
        IsActive = menu.IsActive,
        CreationTime = menu.CreationTime,
        CreatorId = menu.CreatorId,
        LastModificationTime = menu.LastModificationTime,
        LastModifierId = menu.LastModifierId,
        IsDeleted = menu.IsDeleted,
        DeleterId = menu.DeleterId,
        DeletionTime = menu.DeletionTime
    };

    public static MenuListDto ToListDto(this Menu menu) => new()
    {
        Id = menu.Id,
        ContextType = menu.ContextType,
        ContextId = menu.ContextId,
        Name = menu.Name,
        DisplayName = menu.DisplayName,
        IsActive = menu.IsActive
    };

    public static MenuItemDto ToDto(this MenuItem item) => new()
    {
        Id = item.Id,
        TenantId = item.TenantId,
        MenuId = item.MenuId,
        ParentId = item.ParentId,
        Name = item.Name,
        DisplayName = item.DisplayName,
        Slug = item.Slug,
        Description = item.Description,
        DisplayOrder = item.DisplayOrder,
        Kind = item.Kind,
        DisplayType = item.DisplayType,
        Url = item.Url,
        LinkTarget = item.LinkTarget,
        TargetType = item.TargetType,
        TargetId = item.TargetId,
        Icon = item.Icon,
        CssClass = item.CssClass,
        PermissionName = item.PermissionName,
        ComponentName = item.ComponentName,
        MetadataJson = item.MetadataJson,
        IsActive = item.IsActive,
        IsVisible = item.IsVisible,
        CreationTime = item.CreationTime,
        CreatorId = item.CreatorId,
        LastModificationTime = item.LastModificationTime,
        LastModifierId = item.LastModifierId,
        IsDeleted = item.IsDeleted,
        DeleterId = item.DeleterId,
        DeletionTime = item.DeletionTime
    };

    public static MenuItemTreeDto ToTreeDto(this MenuItem item) => new()
    {
        Id = item.Id,
        TenantId = item.TenantId,
        MenuId = item.MenuId,
        ParentId = item.ParentId,
        Name = item.Name,
        DisplayName = item.DisplayName,
        Slug = item.Slug,
        Description = item.Description,
        DisplayOrder = item.DisplayOrder,
        Kind = item.Kind,
        DisplayType = item.DisplayType,
        Url = item.Url,
        LinkTarget = item.LinkTarget,
        TargetType = item.TargetType,
        TargetId = item.TargetId,
        Icon = item.Icon,
        CssClass = item.CssClass,
        PermissionName = item.PermissionName,
        ComponentName = item.ComponentName,
        MetadataJson = item.MetadataJson,
        IsActive = item.IsActive,
        IsVisible = item.IsVisible,
        CreationTime = item.CreationTime,
        CreatorId = item.CreatorId,
        LastModificationTime = item.LastModificationTime,
        LastModifierId = item.LastModifierId,
        IsDeleted = item.IsDeleted,
        DeleterId = item.DeleterId,
        DeletionTime = item.DeletionTime
    };
}
