using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;

[DependsOn(
    typeof(SufiShortLinksDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiShortLinksEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ShortLinksDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(SufiShortLinksDbProperties.ConnectionStringName, db =>
            {
                db.IsUsedByTenants = true;
            });
        });
    }
}
