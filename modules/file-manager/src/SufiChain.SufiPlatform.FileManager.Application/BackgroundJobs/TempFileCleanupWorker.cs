using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.FileManager.BackgroundJobs;

/// <summary>
/// Background worker that periodically cleans up temporary file items that were never confirmed.
/// </summary>
public class TempFileCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public TempFileCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 1000 * 60 * 60 * 6;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var root = workerContext.ServiceProvider;
        var featureChecker = root.GetRequiredService<IFeatureChecker>();
        if (!await featureChecker.IsEnabledAsync(SufiFileManagerFeatures.Enable) ||
            !await featureChecker.IsEnabledAsync(SufiFileManagerFeatures.FileItems))
        {
            Logger.LogDebug("File Manager or file items feature is disabled. Skipping temporary file cleanup.");
            return;
        }

        Logger.LogInformation("Starting temporary file cleanup job");

        try
        {
            var totalDeleted = 0;
            var totalFailed = 0;

            var tenantRepositoryAvailable = root.GetService<ITenantRepository>() != null;
            if (tenantRepositoryAvailable)
            {
                List<Tenant> tenants;
                using (var hostScope = ServiceScopeFactory.CreateScope())
                {
                    var hostProvider = hostScope.ServiceProvider;
                    var currentTenant = hostProvider.GetRequiredService<ICurrentTenant>();
                    using (currentTenant.Change(null))
                    {
                        tenants = await hostProvider
                            .GetRequiredService<ITenantRepository>()
                            .GetListAsync(includeDetails: false);
                        var (hostDeleted, hostFailed) =
                            await CleanupTempFilesForCurrentContextAsync(hostProvider);
                        totalDeleted += hostDeleted;
                        totalFailed += hostFailed;
                    }
                }

                foreach (var tenant in tenants)
                {
                    using (var tenantScope = ServiceScopeFactory.CreateScope())
                    {
                        var tenantProvider = tenantScope.ServiceProvider;
                        var currentTenant = tenantProvider.GetRequiredService<ICurrentTenant>();
                        using (currentTenant.Change(tenant.Id))
                        {
                            var (deleted, failed) =
                                await CleanupTempFilesForCurrentContextAsync(tenantProvider);
                            totalDeleted += deleted;
                            totalFailed += failed;
                        }
                    }
                }
            }
            else
            {
                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var (deleted, failed) =
                        await CleanupTempFilesForCurrentContextAsync(scope.ServiceProvider);
                    totalDeleted += deleted;
                    totalFailed += failed;
                }
            }

            Logger.LogInformation(
                "Temporary file cleanup completed: {Deleted} deleted, {Failed} failed",
                totalDeleted,
                totalFailed);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Temporary file cleanup job failed");
        }
    }

    protected virtual async Task<(int Deleted, int Failed)> CleanupTempFilesForCurrentContextAsync(
        IServiceProvider serviceProvider)
    {
        // Each host/tenant pass must own this unit of work and its DbContext. Reusing the
        // worker-tick ServiceProvider across tenants disposed FileManagerDbContext after the
        // first CompleteAsync (SMK-17).
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

        var fileItemRepository = serviceProvider.GetRequiredService<IFileItemRepository>();
        var structureBlobContainerProvider = serviceProvider.GetRequiredService<IStructureBlobContainerProvider>();
        var dataFilter = serviceProvider.GetRequiredService<IDataFilter<IMultiTenant>>();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();

        var retentionDays = await settingProvider.GetAsync<int>(FileManagerSettings.AutoDeleteTempMediaAfterDays);
        if (retentionDays <= 0)
        {
            retentionDays = 7;
        }

        var cutoffTime = DateTime.UtcNow.AddDays(-retentionDays);

        List<FileItem> tempFileItems;
        using (dataFilter.Disable())
        {
            var query = await fileItemRepository.GetQueryableAsync();
            tempFileItems = await fileItemRepository.AsyncExecuter.ToListAsync(
                query.Where(x => x.IsTemp && x.CreationTime < cutoffTime));
        }

        int deletedCount = 0;
        int failedCount = 0;

        foreach (var fileItem in tempFileItems)
        {
            try
            {
                var blobContainer = await structureBlobContainerProvider.GetContainerAsync(
                    fileItem.StructureKey,
                    fileItem.StorageProvider);
                if (await blobContainer.ExistsAsync(fileItem.BlobName))
                {
                    await blobContainer.DeleteAsync(fileItem.BlobName);
                }

                if (!string.IsNullOrEmpty(fileItem.ThumbnailBlobName) &&
                    await blobContainer.ExistsAsync(fileItem.ThumbnailBlobName))
                {
                    await blobContainer.DeleteAsync(fileItem.ThumbnailBlobName);
                }

                await fileItemRepository.DeleteAsync(fileItem);
                deletedCount++;
            }
            catch (Exception ex)
            {
                failedCount++;
                _ = ex;
            }
        }

        await unitOfWork.CompleteAsync();
        return (deletedCount, failedCount);
    }
}
