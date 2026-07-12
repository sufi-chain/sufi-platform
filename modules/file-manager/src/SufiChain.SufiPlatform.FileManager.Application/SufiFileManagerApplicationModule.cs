using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Caching;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BlobStoring.Minio;
using SufiChain.SufiPlatform.BlobStoring.S3Provider;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.FileManager.BackgroundJobs;
using SufiChain.SufiPlatform.FileManager.Caching;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerDomainModule),
    typeof(SufiFileManagerApplicationContractsModule),
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(AbpCachingModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpBlobStoringModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(AbpBlobStoringMinioModule),
    typeof(SufiBlobStoringS3ProviderModule),
    typeof(AbpBackgroundWorkersModule)
    )]
public class SufiFileManagerApplicationModule : AbpModule
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

        context.Services.AddMapperlyObjectMapper<SufiFileManagerApplicationModule>();

        context.Services.AddTransient<IStructureCache, StructureCacheService>();

        context.Services.Replace(
            ServiceDescriptor.Transient<IBlobContainerConfigurationProvider, StructureBlobContainerConfigurationProvider>());
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<TempFileCleanupWorker>();
    }
}