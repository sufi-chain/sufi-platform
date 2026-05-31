using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.FileManager.Blazor.Menus;
using SufiChain.SufiAbp.FileManager.Blazor.Settings;
using SufiChain.SufiAbp.SettingManagement.Blazor.Settings;
using SufiChain.SufiAbp.FileManager.Blazor.Public;
using SufiChain.SufiAbp.FileManager.Blazor.Services;
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager.Blazor;

[DependsOn(
    typeof(SufiAbpFileManagerBlazorPublicModule),
    typeof(SufiChain.SufiAbp.SettingManagement.Blazor.SufiAbpSettingManagementBlazorModule)
)]
public class SufiAbpFileManagerBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register Cropper.js vendor bundle for on-demand image editor loading
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(FileManagerBundles.Cropper, "/_content/SufiChain.SufiAbp.FileManager.Blazor/vendor/cropper.min.css");
            options.ScriptBundles.Add(FileManagerBundles.Cropper, "/_content/SufiChain.SufiAbp.FileManager.Blazor/vendor/cropper.min.js");
        });

       
        // Register JavaScript interop service
        context.Services.AddScoped<FileManagerJsInterop>();
        context.Services.AddScoped<Components.FileManager.ImageEditorInterop>();
        // IFileItemUrlProvider is registered in FileManagerBlazorPublicModule (admin depends on public)

        // Note: IFileUploadAccessTokenProvider is registered by hosting-specific modules:
        // - FileManagerBlazorServerModule for Blazor Server (uses IHttpContextAccessor)
        // - FileManagerBlazorWebAssemblyModule for Blazor WebAssembly (uses IAccessTokenProvider)

        // Register this assembly for Blazor routing
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpFileManagerBlazorModule).Assembly);
        });

        // Register menu contributor
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new FileManagerMenuContributor());
        });

        // Register File Storage settings contributor (adds "File Storage" tab to Settings page)
        Configure<SettingManagementComponentOptions>(options =>
        {
            options.Contributors.Add(new FileManagerGeneralSettingsGroupContributor());
            options.Contributors.Add(new FileManagerStorageSettingsGroupContributor());
            options.Contributors.Add(new FileManagerArchivingSettingsGroupContributor());
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

