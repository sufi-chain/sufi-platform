using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.BlobStoring.Database.MongoDB;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.MongoDB;
using SufiChain.SufiAbp.FileManager.Repositories;
using SufiChain.SufiAbp.FileManager.Storage;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.BlobStoring;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(SufiAbpMongoDbModule),
    typeof(SufiAbpBlobStoringDatabaseMongoDbModule)
)]
public class SufiAbpFileManagerMongoDbModule : AbpModule
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
            options.Databases.Configure(SufiAbpFileManagerDbProperties.ConnectionStringName, db =>
            {
                db.IsUsedByTenants = true;
            });
        });
    }
}

