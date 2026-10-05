namespace SufiChain.SufiPlatform.Menus;

public static class MenusErrorCodes
{
    public const string Namespace = "Sufi.Menus";
    public const string MenuAlreadyExists = Namespace + ":MenuAlreadyExists";
    public const string MenuNotFound = Namespace + ":MenuNotFound";
    public const string MenuItemNotFound = Namespace + ":MenuItemNotFound";
    public const string MenuItemSlugAlreadyExists = Namespace + ":MenuItemSlugAlreadyExists";
    public const string MenuItemCircularReference = Namespace + ":MenuItemCircularReference";
    public const string MenuItemInvalidParent = Namespace + ":MenuItemInvalidParent";
    public const string MenuItemInvalidTarget = Namespace + ":MenuItemInvalidTarget";
    public const string CannotDeleteMenuWithItems = Namespace + ":CannotDeleteMenuWithItems";
    public const string MenuItemInvalidUrl = Namespace + ":MenuItemInvalidUrl";
    public const string CannotMoveMenuItemAcrossMenus = Namespace + ":CannotMoveMenuItemAcrossMenus";
    public const string DisplayNameRequired = Namespace + ":DisplayNameRequired";
    public const string DisplayNameLooksLikeKey = Namespace + ":DisplayNameLooksLikeKey";
    public const string DisplayNameTooLong = Namespace + ":DisplayNameTooLong";
}
