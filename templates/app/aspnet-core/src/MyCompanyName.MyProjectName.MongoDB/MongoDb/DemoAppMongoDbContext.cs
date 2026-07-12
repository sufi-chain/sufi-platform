using Volo.Abp.Data;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiAI.MongoDB;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.AuditLogging.MongoDB;
using SufiChain.SufiPlatform.BackgroundJobs.MongoDB;
using SufiChain.SufiPlatform.Features.MongoDB;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.MongoDB;
using SufiChain.SufiPlatform.Identity.MongoDB;
using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Localization.MongoDB;
using SufiChain.SufiPlatform.OpenIddict.MongoDB;
using SufiChain.SufiPlatform.Permissions.MongoDB;
using SufiChain.SufiPlatform.Settings.MongoDB;
using SufiChain.SufiPlatform.ShortLinks;
using SufiChain.SufiPlatform.ShortLinks.MongoDB.MongoDB;
using SufiChain.SufiPlatform.Tenants.MongoDB;
using Volo.Abp.MongoDB;

namespace MyCompanyName.MyProjectName.MongoDB;

[ConnectionStringName("Default")]
public class DemoAppMongoDbContext : AbpMongoDbContext,
    IFileManagerMongoDbContext,
    ILocalizationManagementMongoDbContext,
    IShortLinkGeneratorMongoDbContext,
    IAIMongoDbContext
{
    // File Manager
    public IMongoCollection<FileItem> FileItems => Collection<FileItem>();
    public IMongoCollection<FileFolder> FileFolders => Collection<FileFolder>();
    public IMongoCollection<FileStructure> FileStructures => Collection<FileStructure>();

    // Localization Management
    public IMongoCollection<LocalizationText> LocalizationTexts => Collection<LocalizationText>();
    public IMongoCollection<LocalizationResource> LocalizationResources => Collection<LocalizationResource>();

    // Short Link Generator
    public IMongoCollection<ShortUrl> ShortUrls => Collection<ShortUrl>();
    public IMongoCollection<ShortUrlClick> ShortUrlClicks => Collection<ShortUrlClick>();

    // AI Management
    public IMongoCollection<Workspace> Workspaces => Collection<Workspace>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        /* Configure Sufi Platform modules */
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureSufiTenants();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureSufiSettings();
        modelBuilder.ConfigureSufiFeatures();
        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureBackgroundJobs();
        modelBuilder.ConfigureSufiOpenIddict();
        
        modelBuilder.ConfigureSufiFileManager();
        modelBuilder.ConfigureSufiLocalization();
        modelBuilder.ConfigureSufiShortLinks();
        modelBuilder.ConfigureSufiAI();

        /* Configure your own collections here */
        //modelBuilder.Entity<YourEntity>(b =>
        //{
        //    b.CollectionName = "YourEntities";
        //});
    }
}