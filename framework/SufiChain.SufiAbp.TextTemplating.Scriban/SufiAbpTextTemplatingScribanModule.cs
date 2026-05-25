using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TextTemplating.Scriban;

[DependsOn(
    typeof(SufiAbpTextTemplatingModule)
)]
public class SufiAbpTextTemplatingScribanModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpTextTemplatingOptions>(options =>
        {
            options.DefaultRenderingEngine = ScribanTemplateRenderingEngine.EngineName;
            options.RenderingEngines[ScribanTemplateRenderingEngine.EngineName] = typeof(ScribanTemplateRenderingEngine);
        });
    }
}
