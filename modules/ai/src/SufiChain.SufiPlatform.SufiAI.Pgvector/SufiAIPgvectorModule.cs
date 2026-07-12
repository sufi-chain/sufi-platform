using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.SufiAI.Pgvector;
using SufiChain.SufiPlatform.SufiAI.RAG;

namespace SufiChain.SufiPlatform.SufiAI;

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
