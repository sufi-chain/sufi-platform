using Volo.Abp.Data;

namespace SufiChain.SufiAbp.OpenIddict;

public static class SufiAbpOpenIddictDbProperties
{
    public static string DbTablePrefix { get; set; } = "OpenIddict.";

    public static string DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiAbpOpenIddict";
}
