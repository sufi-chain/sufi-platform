using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.TextTemplating;

public class SufiTemplateRenderer : ITemplateRenderer, ITransientDependency
{
    protected IServiceScopeFactory ServiceScopeFactory { get; }
    protected ITemplateDefinitionManager TemplateDefinitionManager { get; }
    protected SufiTextTemplatingOptions Options { get; }

    public SufiTemplateRenderer(
        IServiceScopeFactory serviceScopeFactory,
        ITemplateDefinitionManager templateDefinitionManager,
        IOptions<SufiTextTemplatingOptions> options)
    {
        ServiceScopeFactory = serviceScopeFactory;
        TemplateDefinitionManager = templateDefinitionManager;
        Options = options.Value;
    }

    public virtual async Task<string> RenderAsync(
        string templateName,
        object? model = null,
        string? cultureName = null,
        Dictionary<string, object>? globalContext = null)
    {
        var templateDefinition = await TemplateDefinitionManager.GetAsync(templateName);

        var renderEngine = templateDefinition.RenderEngine;

        if (renderEngine.IsNullOrWhiteSpace())
        {
            renderEngine = Options.DefaultRenderingEngine;
        }

        var providerType = Options.RenderingEngines.GetOrDefault(renderEngine!);

        if (providerType != null && typeof(ITemplateRenderingEngine).IsAssignableFrom(providerType))
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var templateRenderingEngine = (ITemplateRenderingEngine)scope.ServiceProvider.GetRequiredService(providerType);
                return await templateRenderingEngine.RenderAsync(templateName, model, cultureName, globalContext);
            }
        }

        throw new AbpException("There is no rendering engine found with template name: " + templateName);
    }
}
