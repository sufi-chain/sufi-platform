using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.AuditLogging.MongoDB;
using SufiChain.SufiPlatform.BackgroundJobs.MongoDB;
using SufiChain.SufiPlatform.Features.MongoDB;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.Identity.MongoDB;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.OpenIddict.MongoDB;
using SufiChain.SufiPlatform.Permissions.MongoDB;
using SufiChain.SufiPlatform.Settings.MongoDB;
using SufiChain.SufiPlatform.ShortLinks.MongoDB;
using SufiChain.SufiPlatform.Tenants.MongoDB;
using SufiChain.SufiPlatform.Users;
using SufiChain.SufiPlatform.BlobDatabase.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName.MongoDB;

[DependsOn(
    typeof(DemoAppDomainModule),
    typeof(SufiPermissionsMongoDbModule),
    typeof(SufiSettingsMongoDbModule),
    typeof(SufiBackgroundJobsMongoDbModule),
    typeof(SufiAuditLoggingMongoDbModule),
    typeof(SufiFeaturesMongoDbModule),
    typeof(SufiIdentityMongoDbModule),
    typeof(SufiUsersMongoDbModule),
    typeof(SufiOpenIddictMongoDbModule),
    typeof(SufiTenantsMongoDbModule),
    typeof(SufiBlobDatabaseDatabaseMongoDbModule),
    typeof(SufiFileManagerMongoDbModule),
    typeof(SufiLocalizationMongoDbModule),
    typeof(SufiShortLinksMongoDbModule),
    typeof(SufiAIMongoDbModule)
)]
public class DemoAppMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<DemoAppMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
        });

        context.Services.AddAlwaysDisableUnitOfWorkTransaction();
        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });
    }
}