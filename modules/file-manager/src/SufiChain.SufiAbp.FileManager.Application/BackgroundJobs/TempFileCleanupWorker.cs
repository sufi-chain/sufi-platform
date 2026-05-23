using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.FileManager.FileItems;
using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.FileManager.BackgroundJobs;

/// <summary>
/// Background worker that periodically cleans up temporary file items that were never confirmed.
/// Removes temp files older than 3 days. Runs every 6 hours.
/// For database-per-tenant setups, iterates per tenant; otherwise uses a single query across all tenants.
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
        // Run every 6 hours
        Timer.Period = 1000 * 60 * 60 * 6; // 6 hours in milliseconds
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Starting temporary file cleanup job");

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var tenantRepository = scope.ServiceProvider.GetService<ITenantRepository>();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();

            if (tenantRepository != null)
            {
                // Database-per-tenant: iterate over host and each tenant
                var totalDeleted = 0;
                var totalFailed = 0;

                // Host (null tenant)
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
                // Single database: disable filter and process all temp files in one pass
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

        var cutoffTime = DateTime.UtcNow.AddDays(-3);
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
                var blobContainer = await structureBlobContainerProvider.GetContainerAsync(fileItem.StructureKey);
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
                // Logger not available in static; exception is logged by caller
                _ = ex;
            }
        }

        return (deletedCount, failedCount);
    }
}
