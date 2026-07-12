using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.OpenIddict;

public static class SufiOpenIddictDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiOpenIddict.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiOpenIddict";
}
