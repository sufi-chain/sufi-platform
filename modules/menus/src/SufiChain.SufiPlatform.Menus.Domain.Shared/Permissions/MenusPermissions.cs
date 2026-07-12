namespace SufiChain.SufiPlatform.Menus.Permissions;

public static class MenusPermissions
{
    public const string GroupName = "SufiMenus";

    public static class Menus
    {
        public const string Default = GroupName + ".Menus";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageItems = Default + ".ManageItems";
    }

    public static string[] GetAll() =>
    [
        Menus.Default,
        Menus.Create,
        Menus.Edit,
        Menus.Delete,
        Menus.ManageItems
    ];
}