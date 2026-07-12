using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.AspNetCore.Mvc;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule)
)]
public class SufiAspNetCoreMvcModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var partManager = context.Services.GetSingletonInstance<ApplicationPartManager>();
        var assembly = typeof(SufiAspNetCoreMvcModule).Assembly;

        if (!partManager.ApplicationParts.OfType<AssemblyPart>().Any(part => part.Assembly == assembly))
        {
            partManager.ApplicationParts.Add(new AssemblyPart(assembly));
        }

        partManager.FeatureProviders.Add(new SufiControllerFeatureProvider());
    }
}
