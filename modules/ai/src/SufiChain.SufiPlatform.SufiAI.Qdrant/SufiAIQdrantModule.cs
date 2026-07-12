using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.SufiAI.Qdrant;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIEntityFrameworkCoreModule)
)]
public class SufiAIQdrantModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IVectorStoreProvider, QdrantVectorStoreProvider>();
    }
}
