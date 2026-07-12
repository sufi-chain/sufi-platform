using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.TextTemplating.Scriban;

[DependsOn(
    typeof(SufiTextTemplatingModule)
)]
public class SufiTextTemplatingScribanModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiTextTemplatingOptions>(options =>
        {
            options.DefaultRenderingEngine = ScribanTemplateRenderingEngine.EngineName;
            options.RenderingEngines[ScribanTemplateRenderingEngine.EngineName] = typeof(ScribanTemplateRenderingEngine);
        });
    }
}
