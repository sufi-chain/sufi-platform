using Volo.Abp.Data;
using MongoDB.Driver;
using SufiChain.SufiAbp.AI.MongoDB;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.AuditLogging.MongoDB;
using SufiChain.SufiAbp.BackgroundJobs.MongoDB;
using SufiChain.SufiAbp.FeatureManagement.MongoDB;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.MongoDB;
using SufiChain.SufiAbp.Identity.MongoDB;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using SufiChain.SufiAbp.LocalizationManagement.MongoDB;
using SufiChain.SufiAbp.OpenIddict.MongoDB;
using SufiChain.SufiAbp.PermissionManagement.MongoDB;
using SufiChain.SufiAbp.SettingManagement.MongoDB;
using SufiChain.SufiAbp.ShortLinkGenerator;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB.MongoDB;
using SufiChain.SufiAbp.TenantManagement.MongoDB;
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

        /* Configure SufiAbp modules */
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureSufiAbpTenantManagement();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureSufiAbpSettingManagement();
        modelBuilder.ConfigureSufiAbpFeatureManagement();
        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureBackgroundJobs();
        modelBuilder.ConfigureSufiAbpOpenIddict();
        
        modelBuilder.ConfigureSufiAbpFileManager();
        modelBuilder.ConfigureSufiAbpLocalizationManagement();
        modelBuilder.ConfigureSufiAbpShortLinkGenerator();
        modelBuilder.ConfigureSufiAI();

        /* Configure your own collections here */
        //modelBuilder.Entity<YourEntity>(b =>
        //{
        //    b.CollectionName = "YourEntities";
        //});
    }
}
