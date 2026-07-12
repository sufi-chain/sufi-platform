using SufiChain.SufiPlatform.Account.Blazor;
using SufiChain.SufiPlatform.SufiAI.Blazor;
using SufiChain.SufiPlatform.AuditLogging.Blazor;
using SufiChain.SufiPlatform.BackgroundJobs.Blazor;
using SufiChain.SufiPlatform.Features.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.Identity.Blazor;
using SufiChain.SufiPlatform.Localization.Blazor;
using SufiChain.SufiPlatform.Menus.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.Tenants.Blazor;

namespace SufiChain.SufiPlatform.BlazorBuildTest.Console;

public static class Program
{
    public static int Main()
    {
        var moduleTypes = new[]
        {
            typeof(SufiAccountBlazorModule),
            typeof(SufiAIBlazorModule),
            typeof(SufiAuditLoggingBlazorModule),
            typeof(SufiBackgroundJobsBlazorModule),
            typeof(SufiFeaturesBlazorModule),
            typeof(SufiFileManagerBlazorModule),
            typeof(SufiIdentityBlazorModule),
            typeof(SufiLocalizationBlazorModule),
            typeof(SufiMenusBlazorModule),
            typeof(SufiSettingsBlazorModule),
            typeof(SufiTenantsBlazorModule),
            typeof(SufiShortLinksBlazorModule)
        };

        foreach (var moduleType in moduleTypes.OrderBy(type => type.FullName))
        {
            var assemblyName = moduleType.Assembly.GetName();
            System.Console.WriteLine($"OK {moduleType.FullName} -> {assemblyName.Name} {assemblyName.Version}");
        }

        System.Console.WriteLine($"Blazor build references confirmed: {moduleTypes.Select(type => type.Assembly).Distinct().Count()} assemblies.");
        return 0;
    }
}