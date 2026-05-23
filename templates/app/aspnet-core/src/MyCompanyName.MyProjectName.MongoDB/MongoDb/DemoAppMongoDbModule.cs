using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AuditLogging.MongoDB;
using SufiChain.SufiAbp.BackgroundJobs.MongoDB;
using SufiChain.SufiAbp.FeatureManagement.MongoDB;
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.Identity.MongoDB;
using SufiChain.SufiAbp.LocalizationManagement;
using SufiChain.SufiAbp.OpenIddict.MongoDB;
using SufiChain.SufiAbp.PermissionManagement.MongoDB;
using SufiChain.SufiAbp.SettingManagement.MongoDB;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;
using SufiChain.SufiAbp.TenantManagement.MongoDB;
using SufiChain.SufiAbp.Users;
using SufiChain.SufiAbp.BlobStoring.Database.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace MyCompanyName.MyProjectName.MongoDB;

[DependsOn(
    typeof(DemoAppDomainModule),
    typeof(SufiAbpPermissionManagementMongoDbModule),
    typeof(SufiAbpSettingManagementMongoDbModule),
    typeof(SufiAbpBackgroundJobsMongoDbModule),
    typeof(SufiAbpAuditLoggingMongoDbModule),
    typeof(SufiAbpFeatureManagementMongoDbModule),
    typeof(SufiAbpIdentityMongoDbModule),
    typeof(SufiAbpUsersMongoDbModule),
    typeof(SufiAbpOpenIddictMongoDbModule),
    typeof(SufiAbpTenantManagementMongoDbModule),
    typeof(SufiAbpBlobStoringDatabaseMongoDbModule),
    typeof(SufiAbpFileManagerMongoDbModule),
    typeof(SufiAbpLocalizationManagementMongoDbModule),
    typeof(SufiAbpShortLinkGeneratorMongoDbModule),
    typeof(SufiAbpAIManagementMongoDbModule)
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
