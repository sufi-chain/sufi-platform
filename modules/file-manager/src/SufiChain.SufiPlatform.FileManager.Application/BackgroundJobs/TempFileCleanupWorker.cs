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

namespace SufiChain.SufiPlatform.FileManager.BackgroundJobs;

/// <summary>
/// Background worker that periodically cleans up temporary file items that were never confirmed.
/// </summary>
public class TempFileCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TempFileCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        Timer.Period = 1000 * 60 * 60 * 6;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var featureChecker = workerContext.ServiceProvider.GetRequiredService<IFeatureChecker>();
        if (!await featureChecker.IsEnabledAsync(SufiFileManagerFeatures.Enable) ||
            !await featureChecker.IsEnabledAsync(SufiFileManagerFeatures.FileItems))
        {
            Logger.LogDebug("File Manager or file items feature is disabled. Skipping temporary file cleanup.");
            return;
        }

        Logger.LogInformation("Starting temporary file cleanup job");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var tenantRepository = scope.ServiceProvider.GetService<ITenantRepository>();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

            if (tenantRepository != null)
            {
                var totalDeleted = 0;
                var totalFailed = 0;

                using (currentTenant.Change(null))
                {
                    var (hostDeleted, hostFailed) = await CleanupTempFilesForCurrentContextAsync(scope.ServiceProvider);
                    totalDeleted += hostDeleted;
                    totalFailed += hostFailed;
                }

                var tenants = await tenantRepository.GetListAsync(includeDetails: false);
                foreach (var tenant in tenants)
                {
                    using (currentTenant.Change(tenant.Id))
                    {
                        var (deleted, failed) = await CleanupTempFilesForCurrentContextAsync(scope.ServiceProvider);
                        totalDeleted += deleted;
                        totalFailed += failed;
                    }
                }

                Logger.LogInformation(
                    "Temporary file cleanup completed: {Deleted} deleted, {Failed} failed",
                    totalDeleted,
                    totalFailed);
            }
            else
            {
                var (deleted, failed) = await CleanupTempFilesForCurrentContextAsync(scope.ServiceProvider);
                Logger.LogInformation(
                    "Temporary file cleanup completed: {Deleted} deleted, {Failed} failed",
                    deleted,
                    failed);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Temporary file cleanup job failed");
        }
    }

    private static async Task<(int Deleted, int Failed)> CleanupTempFilesForCurrentContextAsync(IServiceProvider serviceProvider)
    {
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
        var query = await fileItemRepository.GetQueryableAsync();

        List<FileItem> tempFileItems;
        using (dataFilter.Disable())
        {
            tempFileItems = query
                .Where(x => x.IsTemp && x.CreationTime < cutoffTime)
                .ToList();
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

        return (deletedCount, failedCount);
    }
}
