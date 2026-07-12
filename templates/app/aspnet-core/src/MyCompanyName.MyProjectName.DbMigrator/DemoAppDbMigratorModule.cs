// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.Calendar;
using Volo.Abp.Autofac;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// using SufiChain.SufiPlatform.BlobDatabase.MongoDB;
// </TEMPLATE-REMOVE>
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.Tenants;
using MyCompanyName.MyProjectName.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// using MyCompanyName.MyProjectName.MongoDB;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(DemoAppEntityFrameworkCoreModule),
    typeof(DemoAppApplicationContractsModule),
    // Load application contracts/providers so DbMigrator can seed every Sufi Platform permission.
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiAuditLoggingApplicationModule),
    typeof(SufiBackgroundJobsApplicationModule),
    typeof(SufiIdentityApplicationModule),
    typeof(SufiTenantsApplicationModule),
    typeof(SufiLocalizationApplicationModule),
    typeof(SufiShortLinksApplicationModule),
    typeof(SufiCalendarApplicationModule),
    typeof(SufiAccountApplicationModule),
    typeof(SufiFeaturesApplicationModule),
    typeof(SufiPermissionsApplicationModule),
    typeof(SufiSettingsApplicationModule),
    typeof(SufiAIApplicationModule)
)]
public class DemoAppDbMigratorModule : AbpModule
{

}