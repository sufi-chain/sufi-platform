using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiAbp.BlobStoring.Database;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpBlobStoringModule),
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
