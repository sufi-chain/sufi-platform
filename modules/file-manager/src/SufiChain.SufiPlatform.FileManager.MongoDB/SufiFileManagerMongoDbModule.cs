using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.BlobDatabase.MongoDB;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.MongoDB;
using SufiChain.SufiPlatform.FileManager.Repositories;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BlobStoring;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerDomainModule),
    typeof(AbpMongoDbModule),
    typeof(SufiBlobDatabaseMongoDbModule)
)]
public class SufiFileManagerMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<FileManagerMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
            options.AddRepository<FileItem, MongoFileItemRepository>();
            options.AddRepository<FileFolder, MongoFileFolderRepository>();
            options.AddRepository<FileStructure, MongoFileStructureRepository>();
        });

        // Configure FileManagerContainer to use database blob storage.
        // Host can override by calling Configure<AbpBlobStoringOptions> again (e.g. UseFileSystem, UseAzure).
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<FileManagerContainer>(container =>
            {
                container.UseDatabase();
            });
        });

        // Database-per-tenant: register FileManager so MultiTenantConnectionStringResolver
        // can use tenant-specific connection strings when configured in Tenant Management.
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(SufiFileManagerDbProperties.ConnectionStringName, db =>
            {
                db.IsUsedByTenants = true;
            });
        });
    }
}