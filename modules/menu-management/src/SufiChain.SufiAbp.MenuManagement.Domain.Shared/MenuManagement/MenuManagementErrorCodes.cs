namespace SufiChain.SufiAbp.MenuManagement;

public static class MenuManagementErrorCodes
{
    public const string Namespace = "SufiAbp.MenuManagement";
    public const string MenuAlreadyExists = Namespace + ":MenuAlreadyExists";
    public const string MenuNotFound = Namespace + ":MenuNotFound";
    public const string MenuItemNotFound = Namespace + ":MenuItemNotFound";
    public const string MenuItemSlugAlreadyExists = Namespace + ":MenuItemSlugAlreadyExists";
    public const string MenuItemCircularReference = Namespace + ":MenuItemCircularReference";
    public const string MenuItemInvalidParent = Namespace + ":MenuItemInvalidParent";
    public const string MenuItemInvalidTarget = Namespace + ":MenuItemInvalidTarget";
    public const string MenuItemMetadataTooLong = Namespace + ":MenuItemMetadataTooLong";
    public const string CannotDeleteMenuWithItems = Namespace + ":CannotDeleteMenuWithItems";
    public const string CannotMoveMenuItemAcrossMenus = Namespace + ":CannotMoveMenuItemAcrossMenus";
}
