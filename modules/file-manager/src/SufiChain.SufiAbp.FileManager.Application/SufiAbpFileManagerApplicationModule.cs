using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.LocalizationManagement;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BlobStoring.Minio;
using SufiChain.SufiAbp.BlobStoring.S3Provider;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.FileManager.BackgroundJobs;
using SufiChain.SufiAbp.FileManager.Caching;
using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(SufiAbpFileManagerApplicationContractsModule),
    typeof(SufiAbpLocalizationManagementApplicationContractsModule),
    typeof(AbpCachingModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpBlobStoringMinioModule),
    typeof(SufiAbpBlobStoringS3ProviderModule),
    typeof(AbpBackgroundWorkersModule)
    )]
public class SufiAbpFileManagerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<FileManagerOptions>(configuration.GetSection("FileManager"));
        PostConfigure<FileManagerOptions>(options =>
        {
            if (options.SeedDefaultStructures)
            {
                options.AddDefaultStructures();
            }
        });

        context.Services.AddMapperlyObjectMapper<SufiAbpFileManagerApplicationModule>();

        context.Services.AddTransient<IStructureCache, StructureCacheService>();

        context.Services.Replace(
            ServiceDescriptor.Transient<IBlobContainerConfigurationProvider, StructureBlobContainerConfigurationProvider>());
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<TempFileCleanupWorker>();
    }
}
