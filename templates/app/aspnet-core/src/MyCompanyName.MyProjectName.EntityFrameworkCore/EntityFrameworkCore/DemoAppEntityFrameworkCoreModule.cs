using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;
using SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Features.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.Identity.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.EntityFrameworkCore;
using SufiChain.SufiPlatform.OpenIddict.EntityFrameworkCore;
using SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
using SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore.SqlServer;
namespace MyCompanyName.MyProjectName.EntityFrameworkCore;

[DependsOn(
    typeof(DemoAppDomainModule),
    typeof(DemoAppApplicationContractsModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),

    // Sufi Platform Infrastructure Modules
    typeof(SufiIdentityEntityFrameworkCoreModule),
    typeof(SufiTenantsEntityFrameworkCoreModule),
    typeof(SufiPermissionsEntityFrameworkCoreModule),
    typeof(SufiFeaturesEntityFrameworkCoreModule),
    typeof(SufiSettingsEntityFrameworkCoreModule),
    typeof(SufiAuditLoggingEntityFrameworkCoreModule),
    typeof(SufiBackgroundJobsEntityFrameworkCoreModule),
    typeof(SufiOpenIddictEntityFrameworkCoreModule),
    typeof(SufiUsersEntityFrameworkCoreModule),
    
    // Sufi Platform Business Modules
    typeof(SufiFileManagerEntityFrameworkCoreModule),
    typeof(SufiLocalizationEntityFrameworkCoreModule),
    typeof(SufiShortLinksEntityFrameworkCoreModule),
    typeof(SufiAIEntityFrameworkCoreModule),
    typeof(SufiCalendarEntityFrameworkCoreModule),
    
    typeof(SufiDataModule)
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