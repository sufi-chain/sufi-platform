using Volo.Abp.BlobStoring;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiPlatform.BlobDatabase;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpBlobStoringModule),
    typeof(SufiBlobDatabaseDomainSharedModule)
)]
public class SufiBlobDatabaseDomainModule : AbpModule
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
