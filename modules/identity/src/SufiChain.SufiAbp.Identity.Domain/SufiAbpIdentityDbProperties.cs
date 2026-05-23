namespace SufiChain.SufiAbp.Identity;

public static class SufiAbpIdentityDbProperties
{
    public static string DbTablePrefix { get; set; } = "Identity.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Default";
}
