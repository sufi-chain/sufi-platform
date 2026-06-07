namespace SufiChain.SufiAbp.MenuManagement;

public static class MenuManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "Menus.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "MenuManagement";
}
