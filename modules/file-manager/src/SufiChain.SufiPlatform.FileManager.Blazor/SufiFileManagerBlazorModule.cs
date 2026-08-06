using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Blazor.Menus;
using SufiChain.SufiPlatform.FileManager.Blazor.Settings;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.FileManager.Blazor.Public;
using SufiChain.SufiPlatform.FileManager.Blazor.Services;
using SufiChain.SufiPlatform.Identity.Blazor.Public;
using SufiChain.SufiPlatform.Users.Blazor.Public;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.Blazor;

[DependsOn(
    typeof(SufiFileManagerBlazorPublicModule),
    typeof(SufiUsersBlazorPublicModule),
    typeof(SufiIdentityBlazorPublicModule),
    typeof(SufiChain.SufiPlatform.Settings.Blazor.SufiSettingsBlazorModule)
)]
public class SufiFileManagerBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register Cropper.js vendor bundle for on-demand image editor loading
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(FileManagerBundles.Cropper, "/_content/SufiChain.SufiPlatform.FileManager.Blazor/vendor/cropper.min.css");
            options.ScriptBundles.Add(FileManagerBundles.Cropper, "/_content/SufiChain.SufiPlatform.FileManager.Blazor/vendor/cropper.min.js");
        });

       
        // Register JavaScript interop service
        context.Services.AddScoped<FileManagerJsInterop>();
        context.Services.AddScoped<Components.FileManager.ImageEditorInterop>();
        // IFileItemUrlProvider is registered in FileManagerBlazorPublicModule (admin depends on public)

        // Note: IFileUploadAccessTokenProvider is registered by hosting-specific modules:
        // - FileManagerBlazorServerModule for Blazor Server (uses IHttpContextAccessor)
        // - FileManagerBlazorWebAssemblyModule for Blazor WebAssembly (uses IAccessTokenProvider)

        // Register this assembly for Blazor routing
        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiFileManagerBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new FileManagerMenuContributor());
        });

        // Register File Manager settings contributor (single tab with all File Manager settings)
        Configure<SettingsComponentOptions>(options =>
        {
            options.Contributors.Add(new FileManagerSettingsGroupContributor());
        });
    }
}

/// <summary>
/// Bundle names for File Manager Blazor (vendor assets loaded on demand).
/// </summary>
public static class FileManagerBundles
{
    /// <summary>
    /// Cropper.js script and styles for the image editor.
    /// </summary>
    public const string Cropper = "FileManager.Cropper";
}