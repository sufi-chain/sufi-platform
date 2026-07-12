using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiPlatform.FileManager.Blazor;
using SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly;
// </TEMPLATE-REMOVE>
using SufiChain.SufiTheme.Blazor.WebAssembly;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;
using MyCompanyName.MyProjectName.Blazor.WebSite.Client.Menus;

namespace MyCompanyName.MyProjectName.Blazor.WebSite.Client;

/// <summary>
/// ABP Module for DemoApp Blazor WebSite WebAssembly client.
/// Configures Sufi Platform theme and navigation for the public-facing WebAssembly app.
/// 
/// Key differences from WebApp.Client:
/// - NO admin modules (Identity.Blazor admin, TenantManagement)
/// - Minimal UI modules for public site
/// - Uses public menu contributor
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(DemoAppHttpApiClientModule),
    // Sufi Platform Theme & UI Modules for WebAssembly
    typeof(SufiThemeBlazorWebAssemblyModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager Module (for public file access)
    typeof(SufiFileManagerBlazorModule),
    typeof(SufiFileManagerBlazorWebAssemblyModule)
    // </TEMPLATE-REMOVE>
)]
public class DemoAppBlazorWebSiteClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();
        
        // Configure Sufi Platform router
        Configure<SufiRouterOptions>(options =>
        {
            options.AppAssembly = typeof(DemoAppBlazorWebSiteClientModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(DemoAppBlazorWebSiteClientModule).Assembly);
        });
        
        // Configure Sufi Platform navigation
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DemoAppPublicMenuContributor(builder.Configuration));
        });
    }
}