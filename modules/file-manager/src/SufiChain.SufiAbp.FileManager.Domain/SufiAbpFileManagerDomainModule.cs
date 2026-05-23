using SufiChain.SufiAbp.BlobStoring;
using SufiChain.SufiAbp.BlobStoring.Database;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.FileManager.Workers;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpFileManagerDomainSharedModule),
    typeof(SufiAbpBlobStoringModule),
    typeof(SufiAbpBlobStoringDatabaseDomainModule)
)]
public class SufiAbpFileManagerDomainModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<FileArchivingWorker>();
    }
}
