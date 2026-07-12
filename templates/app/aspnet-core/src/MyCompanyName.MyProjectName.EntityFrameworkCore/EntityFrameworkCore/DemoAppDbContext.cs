using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.AuditLogging.EntityFrameworkCore;
using SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Features.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.Identity.EntityFrameworkCore;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Localization.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.OpenIddict.EntityFrameworkCore;
using SufiChain.SufiPlatform.Permissions.EntityFrameworkCore;
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.ShortLinks.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.AuditLogging;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;
using SufiChain.SufiPlatform.Permissions;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Tenants;

namespace MyCompanyName.MyProjectName.EntityFrameworkCore;

[ReplaceDbContext(typeof(ISufiIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(ISufiPermissionsDbContext))]
[ReplaceDbContext(typeof(ISufiSettingsDbContext))]
[ReplaceDbContext(typeof(ISufiFeaturesDbContext))]
[ReplaceDbContext(typeof(ISufiAuditLoggingDbContext))]
[ReplaceDbContext(typeof(ISufiBackgroundJobsDbContext))]
[ReplaceDbContext(typeof(IOpenIddictDbContext))]
[ReplaceDbContext(typeof(ISufiFileManagerDbContext))]
[ReplaceDbContext(typeof(ISufiBlobDatabaseDbContext))]
[ReplaceDbContext(typeof(ISufiLocalizationDbContext))]
[ReplaceDbContext(typeof(ISufiShortLinksDbContext))]
[ReplaceDbContext(typeof(IAIDbContext))]
[ReplaceDbContext(typeof(ICalendarDbContext))]
[ConnectionStringName("Default")]
public class DemoAppDbContext :
    AbpDbContext<DemoAppDbContext>,
    ISufiIdentityDbContext,
    ITenantManagementDbContext,
    ISufiPermissionsDbContext,
    ISufiSettingsDbContext,
    ISufiFeaturesDbContext,
    ISufiAuditLoggingDbContext,
    ISufiBackgroundJobsDbContext,
    IOpenIddictDbContext,
    ISufiFileManagerDbContext,
    ISufiBlobDatabaseDbContext,
    ISufiLocalizationDbContext,
    ISufiShortLinksDbContext,
    IAIDbContext,
    ICalendarDbContext
{
    #region Entities from ABP modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; } = null!;
    public DbSet<IdentityRole> Roles { get; set; } = null!;
    public DbSet<IdentityClaimType> ClaimTypes { get; set; } = null!;
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; } = null!;
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; } = null!;
    public DbSet<IdentityLinkUser> LinkUsers { get; set; } = null!;
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; } = null!;
    public DbSet<IdentitySession> Sessions { get; set; } = null!;

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; } = null!;

    // Permission Management
    public DbSet<PermissionGrant> PermissionGrants { get; set; } = null!;
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; } = null!;
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; } = null!;
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; } = null!;

    // Setting Management
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<SettingDefinitionRecord> SettingDefinitions { get; set; } = null!;
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; } = null!;

    // Feature Management
    public DbSet<FeatureValue> FeatureValues { get; set; } = null!;
    public DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; set; } = null!;
    public DbSet<FeatureDefinitionRecord> Features { get; set; } = null!;

    // Audit Logging
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<AuditLogAction> AuditLogActions { get; set; } = null!;
    public DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; set; } = null!;

    // Background Jobs
    public DbSet<BackgroundJobRecord> BackgroundJobs { get; set; } = null!;

    // OpenIddict
    public DbSet<OpenIddictApplication> Applications { get; set; } = null!;
    public DbSet<OpenIddictAuthorization> Authorizations { get; set; } = null!;
    public DbSet<OpenIddictScope> Scopes { get; set; } = null!;
    public DbSet<OpenIddictToken> Tokens { get; set; } = null!;

    // File Manager
    public DbSet<FileItem> FileItems { get; set; } = null!;
    public DbSet<FileStructure> FileStructures { get; set; } = null!;
    public DbSet<FileFolder> FileFolders { get; set; } = null!;
    public DbSet<FolderPermission> FolderPermissions { get; set; } = null!;

    // Database Blob Storage
    public DbSet<DatabaseBlobContainer> BlobContainers { get; set; } = null!;
    public DbSet<DatabaseBlob> Blobs { get; set; } = null!;

    // Localization Management
    public DbSet<LocalizationResource> LocalizationResources { get; set; } = null!;
    public DbSet<LocalizationText> LocalizationTexts { get; set; } = null!;

    // Short Link Generator
    public DbSet<ShortUrl> ShortUrls { get; set; } = null!;
    public DbSet<ShortUrlClick> ShortUrlClicks { get; set; } = null!;

    // AI Management
    public DbSet<SufiChain.SufiPlatform.SufiAI.Workspaces.Workspace> Workspaces { get; set; } = null!;
    public DbSet<SufiChain.SufiPlatform.SufiAI.MCP.Entities.MCPServer> MCPServers { get; set; } = null!;

    // Calendar
    public DbSet<SufiChain.SufiPlatform.Calendar.Calendars.Calendar> Calendars { get; set; } = null!;
    public DbSet<WorkingHourRule> WorkingHourRules { get; set; } = null!;
    public DbSet<CalendarException> CalendarExceptions { get; set; } = null!;
    public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!;
    public DbSet<EventOccurrenceException> EventOccurrenceExceptions { get; set; } = null!;
    public DbSet<EventAttendee> EventAttendees { get; set; } = null!;
    public DbSet<EventReminder> EventReminders { get; set; } = null!;

    #endregion

    public DemoAppDbContext(DbContextOptions<DemoAppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Configure Sufi Platform modules */
        builder.ConfigureSufiIdentity();
        builder.ConfigureSufiTenants();
        builder.ConfigureSufiPermissions();
        builder.ConfigureSufiSettings();
        builder.ConfigureSufiFeatures();
        builder.ConfigureSufiAuditLogging();
        builder.ConfigureSufiBackgroundJobs();
        builder.ConfigureSufiOpenIddict();
        
        builder.ConfigureSufiFileManager();
        builder.ConfigureSufiBlobDatabaseDatabase();
        builder.ConfigureSufiLocalization();
        builder.ConfigureSufiShortLinks();
        builder.ConfigureSufiAI();
        builder.ConfigureSufiCalendar();

        /* Configure your own tables/entities inside here */
        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(DemoAppConsts.DbTablePrefix + "YourEntities", DemoAppConsts.DbSchema);
        //    b.ConfigureByConvention();
        //    //...
        //});
    }
}