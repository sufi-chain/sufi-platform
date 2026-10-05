using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SufiChain.SufiPlatform.SufiAI.Application;
using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAISqliteTestModule),
    typeof(SufiAIApplicationModule))]
public class SufiAIApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient();
        context.Services.Replace(
            ServiceDescriptor.Transient<IAIHooshvareModelSelectionPolicyProvider, TestHooshvareModelSelectionPolicyProvider>());
        context.Services.TryAddSingleton(Substitute.For<IBlobContainer>());
    }
}
