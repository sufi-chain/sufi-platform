using System.Reflection;
using SufiChain.SufiAbp.Account.Blazor;
using SufiChain.SufiAbp.AIManagement.Blazor;
using SufiChain.SufiAbp.AuditLogging.Blazor;
using SufiChain.SufiAbp.BackgroundJobs.Blazor;
using SufiChain.SufiAbp.FeatureManagement.Blazor;
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.Identity.Blazor;
using SufiChain.SufiAbp.LocalizationManagement.Blazor;
using SufiChain.SufiAbp.SettingManagement.Blazor;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.ShortLinkGenerator.Blazor;
using SufiChain.SufiAbp.TenantManagement.Blazor;

namespace SufiChain.SufiAbp.BlazorBuildTest.Console;

public static class Program
{
    public static int Main()
    {
        var moduleTypes = new[]
        {
            typeof(SufiAbpAccountBlazorModule),
            typeof(SufiAbpAIManagementBlazorModule),
            typeof(SufiAbpAuditLoggingBlazorModule),
            typeof(SufiAbpBackgroundJobsBlazorModule),
            typeof(SufiAbpFeatureManagementBlazorModule),
            typeof(SufiAbpFileManagerBlazorModule),
            typeof(SufiAbpIdentityBlazorModule),
            typeof(SufiAbpLocalizationManagementBlazorModule),
            typeof(SufiAbpSettingManagementBlazorModule),
            typeof(SufiAbpTenantManagementBlazorModule),
            typeof(SufiAbpShortLinkGeneratorBlazorModule)
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
