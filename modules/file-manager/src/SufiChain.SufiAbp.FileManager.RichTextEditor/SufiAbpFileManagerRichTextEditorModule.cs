using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiBlazor;
using SufiChain.SufiBlazor.Contracts.Editors;
using SufiChain.SufiAbp.FileManager.Blazor;
using SufiChain.SufiAbp.FileManager.RichTextEditor.Toolbar;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager.RichTextEditor;

[DependsOn(
    typeof(SufiAbpFileManagerBlazorModule)
)]
public class SufiAbpFileManagerRichTextEditorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register the dialog service (scoped to handle Blazor component lifecycle)
        context.Services.AddScoped<FileGalleryDialogService>();
        context.Services.AddScoped<IFileGalleryDialogService>(sabp => sabp.GetRequiredService<FileGalleryDialogService>());

        // Register the toolbar contributor
        context.Services.AddRteToolbarContributor<FileManagerToolbarContributor>();

        // Register this assembly for Blazor routing (demo pages)
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpFileManagerRichTextEditorModule).Assembly);
        });
    }
}
