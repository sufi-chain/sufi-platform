using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;
using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.EntityFrameworkCore;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.EntityFrameworkCore;
using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;
using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;
using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.Users;
using SufiChain.SufiAbp.EntityFrameworkCore;
using SufiChain.SufiAbp.EntityFrameworkCore.SqlServer;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName.EntityFrameworkCore;

[DependsOn(
    typeof(DemoAppDomainModule),
    typeof(DemoAppApplicationContractsModule),
    typeof(SufiAbpEntityFrameworkCoreSqlServerModule),

    // SufiAbp Infrastructure Modules
    typeof(SufiAbpIdentityEntityFrameworkCoreModule),
    typeof(SufiAbpTenantManagementEntityFrameworkCoreModule),
    typeof(SufiAbpPermissionManagementEntityFrameworkCoreModule),
    typeof(SufiAbpFeatureManagementEntityFrameworkCoreModule),
    typeof(SufiAbpSettingManagementEntityFrameworkCoreModule),
    typeof(SufiAbpAuditLoggingEntityFrameworkCoreModule),
    typeof(SufiAbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(SufiAbpOpenIddictEntityFrameworkCoreModule),
    typeof(SufiAbpUsersEntityFrameworkCoreModule),
    
    // SufiAbp Business Modules
    typeof(SufiAbpFileManagerEntityFrameworkCoreModule),
    typeof(SufiAbpLocalizationManagementEntityFrameworkCoreModule),
    typeof(SufiAbpShortLinkGeneratorEntityFrameworkCoreModule),
    typeof(SufiAIEntityFrameworkCoreModule),
    typeof(SufiAbpCalendarEntityFrameworkCoreModule),
    
    typeof(SufiAbpDataModule)
)]
public class DemoAppEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        DemoAppEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<DemoAppDbContext>(options =>
        {
            /* Remove "includeAllEntities: true" to create
             * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            /* The main point to change your DBMS.
             * See also DemoAppDbContextFactory for EF Core tooling. */
            options.UseSqlServer();
        });
    }
}
