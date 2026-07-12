using SufiChain.SufiAbp.BlobStoring.Database;
using SufiChain.SufiAbp.FileManager.Workers;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.BlobStoring;
namespace SufiChain.SufiAbp.FileManager;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAbpFileManagerDomainSharedModule),
    typeof(AbpBlobStoringModule),
    typeof(SufiAbpBlobStoringDatabaseDomainModule)
)]
public class SufiAbpFileManagerDomainModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<FileArchivingWorker>();
    }
}
