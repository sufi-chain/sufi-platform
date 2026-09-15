using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Content.Rendering;

public class ContentRenderingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<IMarkdownRenderer, MarkdigMarkdownRenderer>();
        context.Services.AddSingleton<IHtmlSanitizer, HtmlSanitizerService>();
    }
}
