using SufiChain.SufiAbp.BlobStoring;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BlobStoring.Database;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpBlobStoringModule),
    typeof(SufiAbpBlobStoringDatabaseDomainSharedModule)
)]
public class SufiAbpBlobStoringDatabaseDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                if (container.ProviderType == null)
                {
                    container.UseDatabase();
                }
            });
        });
    }
}
