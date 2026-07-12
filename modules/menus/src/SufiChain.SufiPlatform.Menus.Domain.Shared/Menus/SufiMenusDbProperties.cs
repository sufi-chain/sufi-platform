namespace SufiChain.SufiPlatform.Menus;

public static class SufiMenusDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiMenus.";
    public static string? DbSchema { get; set; } = null;
    public const string ConnectionStringName = "SufiMenus";
}