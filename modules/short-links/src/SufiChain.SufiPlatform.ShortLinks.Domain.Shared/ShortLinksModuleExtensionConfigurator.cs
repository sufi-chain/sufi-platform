using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.ShortLinks;

public static class ShortLinksModuleExtensionConfigurator
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
            ConfigureExistingProperties();
            ConfigureExtraProperties();
        });
    }

    private static void ConfigureExistingProperties()
    {
        // Configure existing properties if needed
    }

    private static void ConfigureExtraProperties()
    {
        // Configure extra properties if needed
    }
}

