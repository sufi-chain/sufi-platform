// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.BackgroundJobs;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// using SufiChain.SufiAbp.BlobStoring.Database.MongoDB;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.TenantManagement;
using MyCompanyName.MyProjectName.EntityFrameworkCore;
// </TEMPLATE-REMOVE>
// <TEMPLATE-REMOVE IF-NOT="db:mongodb">
// using MyCompanyName.MyProjectName.MongoDB;
// </TEMPLATE-REMOVE>
using SufiChain.SufiAbp.Autofac;
// <TEMPLATE-REMOVE IF-NOT="db:efcore">
using Volo.Abp.Modularity;

namespace MyCompanyName.MyProjectName.DbMigrator;

[DependsOn(
    typeof(SufiAbpAutofacModule),
    typeof(DemoAppEntityFrameworkCoreModule),
    typeof(DemoAppApplicationContractsModule),
    // Load application contracts/providers so DbMigrator can seed every SufiAbp permission.
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpAuditLoggingApplicationModule),
    typeof(SufiAbpBackgroundJobsApplicationModule),
    typeof(SufiAbpIdentityApplicationModule),
    typeof(SufiAbpTenantManagementApplicationModule),
    typeof(SufiAbpLocalizationManagementApplicationModule),
    typeof(SufiAbpShortLinkGeneratorApplicationModule),
    typeof(SufiAbpAccountApplicationModule),
    typeof(SufiAbpFeatureManagementApplicationModule),
    typeof(SufiAbpPermissionManagementApplicationModule),
    typeof(SufiAbpSettingManagementApplicationModule),
    typeof(SufiAbpAIManagementApplicationModule)
)]
public class DemoAppDbMigratorModule : AbpModule
{

}
