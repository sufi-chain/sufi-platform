using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.Blazor.WebAssembly;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
using SufiChain.SufiAbp.FileManager.Demo;
// </TEMPLATE-REMOVE>
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.Identity.Blazor;
// <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
using SufiChain.SufiAbp.TenantManagement.Blazor;
// </TEMPLATE-REMOVE>
using SufiChain.SufiTheme.Blazor.WebAssembly;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;
using MyCompanyName.MyProjectName.Menus;

namespace MyCompanyName.MyProjectName;

/// <summary>
/// Blazor WebAssembly client module for the WebApp template.
/// Configures SufiAbp theme and navigation for WebAssembly hosting.
/// NOTE: SufiAbpAccountBlazorModule is Server-only (uses SignInManager/AspNetCore.Identity)
/// and is NOT included here — login pages are served by the Server project.
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    // <TEMPLATE-REMOVE IF="arch:webapp">
    typeof(DemoAppHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // SufiAbp Theme & UI Modules for WebAssembly
    typeof(SufiThemeBlazorWebAssemblyModule),
    typeof(SufiAbpIdentityBlazorModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager Module (UI and WebAssembly-specific)
    typeof(SufiAbpFileManagerBlazorModule),
    typeof(SufiAbpFileManagerBlazorWebAssemblyModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
    typeof(SufiAbpFileManagerDemoModule),
    // </TEMPLATE-REMOVE>
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:tenant-management">
    typeof(SufiAbpTenantManagementBlazorModule)
    // </TEMPLATE-REMOVE>
)]
public class DemoAppClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        // Configure SufiAbp router
        Configure<SufiAbpRouterOptions>(options =>
        {
        });

        // Configure SufiAbp navigation
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DemoAppMenuContributor(builder.Configuration));
        });
    }
}
