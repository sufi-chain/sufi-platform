using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.EntityFrameworkCore;
using SufiChain.SufiAbp.AuditLogging.EntityFrameworkCore;
using SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.Identity.EntityFrameworkCore;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.OpenIddict.EntityFrameworkCore;
using SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;
using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.AuditLogging;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.OpenIddict.Applications;
using SufiChain.SufiAbp.OpenIddict.Authorizations;
using SufiChain.SufiAbp.OpenIddict.Scopes;
using SufiChain.SufiAbp.OpenIddict.Tokens;
using SufiChain.SufiAbp.PermissionManagement;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.TenantManagement;

namespace MyCompanyName.MyProjectName.EntityFrameworkCore;

[ReplaceDbContext(typeof(ISufiAbpIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ReplaceDbContext(typeof(ISufiAbpPermissionManagementDbContext))]
[ReplaceDbContext(typeof(ISufiAbpSettingManagementDbContext))]
[ReplaceDbContext(typeof(ISufiAbpFeatureManagementDbContext))]
[ReplaceDbContext(typeof(ISufiAbpAuditLoggingDbContext))]
[ReplaceDbContext(typeof(ISufiAbpBackgroundJobsDbContext))]
[ReplaceDbContext(typeof(IOpenIddictDbContext))]
[ReplaceDbContext(typeof(ISufiAbpFileManagerDbContext))]
[ReplaceDbContext(typeof(ISufiAbpBlobStoringDbContext))]
[ReplaceDbContext(typeof(ISufiAbpLocalizationManagementDbContext))]
[ReplaceDbContext(typeof(ISufiAbpShortLinkGeneratorDbContext))]
[ReplaceDbContext(typeof(IAIDbContext))]
[ReplaceDbContext(typeof(ICalendarDbContext))]
[ConnectionStringName("Default")]
public class DemoAppDbContext :
    AbpDbContext<DemoAppDbContext>,
    ISufiAbpIdentityDbContext,
    ITenantManagementDbContext,
    ISufiAbpPermissionManagementDbContext,
    ISufiAbpSettingManagementDbContext,
    ISufiAbpFeatureManagementDbContext,
    ISufiAbpAuditLoggingDbContext,
    ISufiAbpBackgroundJobsDbContext,
    IOpenIddictDbContext,
    ISufiAbpFileManagerDbContext,
    ISufiAbpBlobStoringDbContext,
    ISufiAbpLocalizationManagementDbContext,
    ISufiAbpShortLinkGeneratorDbContext,
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
    public DbSet<SufiChain.SufiAbp.AI.Workspaces.Workspace> Workspaces { get; set; } = null!;
    public DbSet<SufiChain.SufiAbp.AI.MCP.Entities.MCPServer> MCPServers { get; set; } = null!;

    // Calendar
    public DbSet<SufiChain.SufiAbp.Calendar.Calendars.Calendar> Calendars { get; set; } = null!;
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

        /* Configure SufiAbp modules */
        builder.ConfigureSufiAbpIdentity();
        builder.ConfigureSufiAbpTenantManagement();
        builder.ConfigureSufiAbpPermissionManagement();
        builder.ConfigureSufiAbpSettingManagement();
        builder.ConfigureSufiAbpFeatureManagement();
        builder.ConfigureSufiAbpAuditLogging();
        builder.ConfigureSufiAbpBackgroundJobs();
        builder.ConfigureSufiAbpOpenIddict();
        
        builder.ConfigureSufiAbpFileManager();
        builder.ConfigureSufiAbpBlobStoringDatabase();
        builder.ConfigureSufiAbpLocalizationManagement();
        builder.ConfigureSufiAbpShortLinkGenerator();
        builder.ConfigureSufiAI();
        builder.ConfigureSufiAbpCalendar();

        /* Configure your own tables/entities inside here */
        //builder.Entity<YourEntity>(b =>
        //{
        //    b.ToTable(DemoAppConsts.DbTablePrefix + "YourEntities", DemoAppConsts.DbSchema);
        //    b.ConfigureByConvention();
        //    //...
        //});
    }
}
