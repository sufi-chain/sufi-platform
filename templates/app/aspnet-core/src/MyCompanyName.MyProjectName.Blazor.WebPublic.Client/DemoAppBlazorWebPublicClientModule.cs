using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
// <TEMPLATE-REMOVE IF-NOT="module:file-manager">
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.Blazor.WebAssembly;
// </TEMPLATE-REMOVE>
using SufiChain.KomTheme.Blazor.WebAssembly;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Modularity;
using MyCompanyName.MyProjectName.Blazor.WebPublic.Client.Menus;

namespace MyCompanyName.MyProjectName.Blazor.WebPublic.Client;

/// <summary>
/// ABP Module for DemoApp Blazor WebPublic WebAssembly client.
/// Configures SufiAbp theme and navigation for the public-facing WebAssembly app.
/// 
/// Key differences from WebApp.Client:
/// - NO admin modules (Identity.Blazor admin, TenantManagement)
/// - Minimal UI modules for public site
/// - Uses public menu contributor
/// </summary>
[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(DemoAppHttpApiClientModule),
    // SufiAbp Theme & UI Modules for WebAssembly
    typeof(KomThemeBlazorWebAssemblyModule),
    // <TEMPLATE-REMOVE IF-NOT="module:file-manager">
    // File Manager Module (for public file access)
    typeof(SufiAbpFileManagerBlazorModule),
    typeof(SufiAbpFileManagerBlazorWebAssemblyModule)
    // </TEMPLATE-REMOVE>
)]
public class DemoAppBlazorWebPublicClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();
        var builder = context.Services.GetSingletonInstance<WebAssemblyHostBuilder>();
        
        // Configure SufiAbp router
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(DemoAppBlazorWebPublicClientModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(DemoAppBlazorWebPublicClientModule).Assembly);
        });
        
        // Configure SufiAbp navigation
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new DemoAppPublicMenuContributor(builder.Configuration));
        });
    }
}
