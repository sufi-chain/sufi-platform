namespace SufiChain.SufiPlatform.Identity;

public static class SufiIdentityDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiIdentity.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Default";
}
