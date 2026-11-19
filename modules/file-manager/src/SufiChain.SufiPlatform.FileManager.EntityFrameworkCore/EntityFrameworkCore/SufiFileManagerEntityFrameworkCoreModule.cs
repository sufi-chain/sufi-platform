using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Repositories;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BlobStoring;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;

[DependsOn(
    typeof(SufiFileManagerDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(SufiBlobDatabaseEntityFrameworkCoreModule)
)]
public class SufiFileManagerEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<FileManagerDbContext>(options =>
        {
            options.AddDefaultRepositories<ISufiFileManagerDbContext>(includeAllEntities: true);

            // Register custom repositories
            options.AddRepository<FileItem, EfCoreFileItemRepository>();
            options.AddRepository<FileStructure, EfCoreFileStructureRepository>();
            options.AddRepository<FileFolder, EfCoreFileFolderRepository>();
        });

        // Transient so the first context built in a request cannot be reused after
        // the tenant becomes current. The unit of work caches the instance it
        // creates for the connection string it just resolved. A scoped registration
        // pinned File Manager to the host SufiFileManager catalog when that first
        // context was opened before the tenant switch, while FileItem.TenantId was
        // still stamped with the tenant.
        context.Services.Replace(ServiceDescriptor.Transient<ISufiFileManagerDbContext, FileManagerDbContext>());

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