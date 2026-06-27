using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.AI.Pgvector;
using SufiChain.SufiAbp.AI.RAG;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIDomainModule),
    typeof(SufiAIEntityFrameworkCoreModule)
)]
public class SufiAIPgvectorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IVectorStoreProvider, PgvectorVectorStoreProvider>();
    }
}
