using Microsoft.Extensions.DependencyInjection;
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
using SufiChain.SufiAbp.BlobStoring;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.BlobStoring.FileSystem;
using SufiChain.SufiAbp.BackgroundWorkers;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpFileManagerDomainModule),
    typeof(SufiAbpFileManagerApplicationContractsModule),
    typeof(SufiAbpCachingModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpBlobStoringModule),
    typeof(SufiAbpBlobStoringFileSystemModule),
    typeof(AbpBlobStoringMinioModule),
    typeof(SufiAbpBlobStoringS3ProviderModule),
    typeof(SufiAbpBackgroundWorkersModule)
    )]
public class SufiAbpFileManagerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
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
