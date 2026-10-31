using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using SufiChain.SufiPlatform.SufiAI.Application;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;
using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAISqliteTestModule),
    typeof(SufiAIOpenRouterModule),
    typeof(SufiAIApplicationModule))]
public class SufiAIApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient();
        // Default suite stays air-gapped. Tests that need live HTTP construct their own handler.
        context.Services.RemoveAll<IModelCatalogProvider>();
        context.Services.AddTransient<IModelCatalogProvider, FakeOpenRouterModelCatalogProvider>();
        context.Services.Replace(
            ServiceDescriptor.Transient<IAIHooshvareModelSelectionPolicyProvider, TestHooshvareModelSelectionPolicyProvider>());
        context.Services.TryAddSingleton(Substitute.For<IBlobContainer>());
        // This host has no settings database. The settings domain module replaces ABP's null
        // stores with implementations that need those repositories.
        context.Services.Replace(
            ServiceDescriptor.Singleton<IDynamicSettingDefinitionStore, NullDynamicSettingDefinitionStore>());
        context.Services.Replace(ServiceDescriptor.Transient<ISettingStore, NullSettingStore>());
    }
}
