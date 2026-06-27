using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.AI.Qdrant;
using SufiChain.SufiAbp.AI.RAG;

namespace SufiChain.SufiAbp.AI;

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
