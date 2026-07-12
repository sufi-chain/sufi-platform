using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.AuditLogging.MongoDB;
using SufiChain.SufiPlatform.BackgroundJobs.MongoDB;
using SufiChain.SufiPlatform.BlobDatabase.MongoDB;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Features.MongoDB;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.MongoDB;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Menus;
using SufiChain.SufiPlatform.Menus.MongoDB;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.OpenIddict.MongoDB;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Permissions.Identity;
using SufiChain.SufiPlatform.Permissions.MongoDB;
using SufiChain.SufiPlatform.Permissions.OpenIddict;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.MongoDB;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.ShortLinks.MongoDB;
using SufiChain.SufiPlatform.Tags;
using SufiChain.SufiPlatform.Tags.MongoDB;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.MongoDB;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.Modularity;

using Volo.Abp.Autofac;
using Volo.Abp.Swashbuckle;
using Volo.Abp.AspNetCore.Serilog;
namespace SufiChain.SufiPlatform.SmokeTest.Console;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(SufiOpenIddictAspNetCoreModule),
    typeof(SufiAccountApplicationModule),
    typeof(SufiAccountHttpApiModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAIHttpApiModule),
    typeof(SufiAIMongoDbModule),
    typeof(SufiAuditLoggingApplicationModule),
    typeof(SufiAuditLoggingHttpApiModule),
    typeof(SufiAuditLoggingMongoDbModule),
    typeof(SufiBackgroundJobsApplicationModule),
    typeof(SufiBackgroundJobsHttpApiModule),
    typeof(SufiBackgroundJobsMongoDbModule),
    typeof(SufiBlobDatabaseMongoDbModule),
    typeof(SufiFeaturesApplicationModule),
    typeof(SufiFeaturesHttpApiModule),
    typeof(SufiFeaturesMongoDbModule),
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiFileManagerHttpApiModule),
    typeof(SufiFileManagerMongoDbModule),
    typeof(SufiIdentityApplicationModule),
    typeof(SufiIdentityHttpApiModule),
    typeof(SufiIdentityMongoDbModule),
    typeof(SufiLocalizationApplicationModule),
    typeof(SufiLocalizationHttpApiModule),
    typeof(SufiLocalizationMongoDbModule),
    typeof(SufiMenusApplicationModule),
    typeof(SufiMenusHttpApiModule),
    typeof(SufiMenusMongoDbModule),
    typeof(SufiOpenIddictMongoDbModule),
    typeof(SufiPermissionsApplicationModule),
    typeof(SufiPermissionsHttpApiModule),
    typeof(SufiPermissionsDomainIdentityModule),
    typeof(SufiPermissionsDomainOpenIddictModule),
    typeof(SufiPermissionsMongoDbModule),
    typeof(SufiSettingsApplicationModule),
    typeof(SufiSettingsHttpApiModule),
    typeof(SufiSettingsMongoDbModule),
    typeof(SufiShortLinksApplicationModule),
    typeof(SufiShortLinksHttpApiModule),
    typeof(SufiShortLinksMongoDbModule),
    typeof(SufiTenantsApplicationModule),
    typeof(SufiTenantsHttpApiModule),
    typeof(SufiTenantsMongoDbModule),
    typeof(SufiTagsApplicationModule),
    typeof(SufiTagsHttpApiModule),
    typeof(SufiTagsMongoDbModule),
    typeof(SufiUsersMongoDbModule)
)]
public class SufiSmokeTestModule : AbpModule
{
}