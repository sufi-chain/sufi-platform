using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.FileManager.Repositories;
using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp.BlobStoring;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(SufiAbpBlobStoringDatabaseEntityFrameworkCoreModule)
)]
public class SufiAbpFileManagerEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FileManagerDbContext>(options =>
        {
            options.AddDefaultRepositories<ISufiAbpFileManagerDbContext>(includeAllEntities: true);

            // Register custom repositories
            options.AddRepository<FileItem, EfCoreFileItemRepository>();
            options.AddRepository<FileStructure, EfCoreFileStructureRepository>();
            options.AddRepository<FileFolder, EfCoreFileFolderRepository>();
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
