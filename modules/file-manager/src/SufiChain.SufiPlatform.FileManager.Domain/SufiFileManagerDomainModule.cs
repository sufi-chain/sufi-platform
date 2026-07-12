using SufiChain.SufiPlatform.BlobDatabase;
using SufiChain.SufiPlatform.FileManager.Workers;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.BlobStoring;
namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiFileManagerDomainSharedModule),
    typeof(AbpBlobStoringModule),
    typeof(SufiBlobDatabaseDomainModule)
)]
public class SufiFileManagerDomainModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<FileArchivingWorker>();
    }
}