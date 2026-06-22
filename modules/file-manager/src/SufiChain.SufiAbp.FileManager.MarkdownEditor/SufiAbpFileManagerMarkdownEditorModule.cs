using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.FileManager.MarkdownEditor.Toolbar;
using SufiChain.SufiAbp.FileManager.RichTextEditor;
using SufiChain.SufiAbp.UI.Routing;
using SufiChain.SufiBlazor;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager.MarkdownEditor;

[DependsOn(typeof(SufiAbpFileManagerRichTextEditorModule))]
public class SufiAbpFileManagerMarkdownEditorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMdToolbarContributor<FileManagerMarkdownToolbarContributor>();

        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpFileManagerMarkdownEditorModule).Assembly);
        });
    }
}
