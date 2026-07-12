using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
using SufiChain.SufiPlatform.FileManager.Demo;
// </TEMPLATE-REMOVE>
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Identity.Blazor;
// <TEMPLATE-REMOVE IF-NOT="module:tenants">
using SufiChain.SufiPlatform.Tenants.Blazor;
// </TEMPLATE-REMOVE>
using SufiChain.SufiTheme.Blazor.WebAssembly;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;
using MyCompanyName.MyProjectName.Menus;

namespace MyCompanyName.MyProjectName;

/// <summary>
/// Blazor WebAssembly client module for the WebApp template.
/// Configures Sufi Platform theme and navigation for WebAssembly hosting.
/// NOTE: SufiAccountBlazorModule is Server-only (uses SignInManager/AspNetCore.Identity)
/// and is NOT included here — login pages are served by the Server project.
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    // <TEMPLATE-REMOVE IF="arch:webapp">
    typeof(DemoAppHttpApiClientModule),
    // </TEMPLATE-REMOVE>
    // Sufi Platform Theme & UI Modules for WebAssembly
    typeof(SufiThemeBlazorWebAssemblyModule),
    typeof(SufiIdentityBlazorModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager Module (UI and WebAssembly-specific)
    typeof(SufiFileManagerBlazorModule),
    typeof(SufiFileManagerBlazorWebAssemblyModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager-demo">
    typeof(SufiFileManagerDemoModule),
    // </TEMPLATE-REMOVE>
    // </TEMPLATE-REMOVE>
    // <TEMPLATE-REMOVE IF-NOT="module:tenants">
    typeof(SufiTenantsBlazorModule)
    // </TEMPLATE-REMOVE>
)]
public class DemoAppClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();

        // Configure Sufi Platform router
        Configure<SufiRouterOptions>(options =>
        {
        });

        // Configure Sufi Platform navigation
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DemoAppMenuContributor(builder.Configuration));
        });
    }
}