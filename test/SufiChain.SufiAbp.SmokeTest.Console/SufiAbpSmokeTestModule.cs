using SufiChain.SufiAbp.Account;
using SufiChain.SufiAbp.AIManagement;
using SufiChain.SufiAbp.AIManagement.MongoDB;
using SufiChain.SufiAbp.AspNetCore.Serilog;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.AuditLogging.MongoDB;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.BackgroundJobs.MongoDB;
using SufiChain.SufiAbp.BlobStoring.Database.MongoDB;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.FeatureManagement.MongoDB;
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.MongoDB;
using SufiChain.SufiAbp.LocalizationManagement;
using SufiChain.SufiAbp.MenuManagement;
using SufiChain.SufiAbp.MenuManagement.MongoDB;
using SufiChain.SufiAbp.OpenIddict;
using SufiChain.SufiAbp.OpenIddict.MongoDB;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.PermissionManagement.Identity;
using SufiChain.SufiAbp.PermissionManagement.MongoDB;
using SufiChain.SufiAbp.PermissionManagement.OpenIddict;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.SettingManagement.MongoDB;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;
using SufiChain.SufiAbp.Swashbuckle;
using SufiChain.SufiAbp.TagsManagement;
using SufiChain.SufiAbp.TagsManagement.MongoDB;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.TenantManagement.MongoDB;
using SufiChain.SufiAbp.Users;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.SmokeTest.Console;

[DependsOn(
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpAspNetCoreSerilogModule),
    typeof(SufiAbpSwashbuckleModule),
    typeof(SufiAbpOpenIddictAspNetCoreModule),
    typeof(SufiAbpAccountApplicationModule),
    typeof(SufiAbpAccountHttpApiModule),
    typeof(SufiAbpAIManagementApplicationModule),
    typeof(SufiAbpAIManagementHttpApiModule),
    typeof(SufiAbpAIManagementMongoDbModule),
    typeof(SufiAbpAuditLoggingApplicationModule),
    typeof(SufiAbpAuditLoggingHttpApiModule),
    typeof(SufiAbpAuditLoggingMongoDbModule),
    typeof(SufiAbpBackgroundJobsApplicationModule),
    typeof(SufiAbpBackgroundJobsHttpApiModule),
    typeof(SufiAbpBackgroundJobsMongoDbModule),
    typeof(SufiAbpBlobStoringDatabaseMongoDbModule),
    typeof(SufiAbpFeatureManagementApplicationModule),
    typeof(SufiAbpFeatureManagementHttpApiModule),
    typeof(SufiAbpFeatureManagementMongoDbModule),
    typeof(SufiAbpFileManagerApplicationModule),
    typeof(SufiAbpFileManagerHttpApiModule),
    typeof(SufiAbpFileManagerMongoDbModule),
    typeof(SufiAbpIdentityApplicationModule),
    typeof(SufiAbpIdentityHttpApiModule),
    typeof(SufiAbpIdentityMongoDbModule),
    typeof(SufiAbpLocalizationManagementApplicationModule),
    typeof(SufiAbpLocalizationManagementHttpApiModule),
    typeof(SufiAbpLocalizationManagementMongoDbModule),
    typeof(SufiAbpMenuManagementApplicationModule),
    typeof(SufiAbpMenuManagementHttpApiModule),
    typeof(SufiAbpMenuManagementMongoDbModule),
    typeof(SufiAbpOpenIddictMongoDbModule),
    typeof(SufiAbpPermissionManagementApplicationModule),
    typeof(SufiAbpPermissionManagementHttpApiModule),
    typeof(SufiAbpPermissionManagementDomainIdentityModule),
    typeof(SufiAbpPermissionManagementDomainOpenIddictModule),
    typeof(SufiAbpPermissionManagementMongoDbModule),
    typeof(SufiAbpSettingManagementApplicationModule),
    typeof(SufiAbpSettingManagementHttpApiModule),
    typeof(SufiAbpSettingManagementMongoDbModule),
    typeof(SufiAbpShortLinkGeneratorApplicationModule),
    typeof(SufiAbpShortLinkGeneratorHttpApiModule),
    typeof(SufiAbpShortLinkGeneratorMongoDbModule),
    typeof(SufiAbpTenantManagementApplicationModule),
    typeof(SufiAbpTenantManagementHttpApiModule),
    typeof(SufiAbpTenantManagementMongoDbModule),
    typeof(SufiAbpTagsManagementApplicationModule),
    typeof(SufiAbpTagsManagementHttpApiModule),
    typeof(SufiAbpTagsManagementMongoDbModule),
    typeof(SufiAbpUsersMongoDbModule)
)]
public class SufiAbpSmokeTestModule : AbpModule
{
}
