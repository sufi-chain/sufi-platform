using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.FileManager.BackgroundJobs;
using SufiChain.SufiAbp.FileManager.Features;
using SufiChain.SufiAbp.FileManager.Settings;
using Volo.Abp.BackgroundWorkers;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace SufiChain.SufiAbp.FileManager.Workers;

/// <summary>
/// Background worker that schedules periodic file archiving.
/// </summary>
public class FileArchivingWorker : AsyncPeriodicBackgroundWorkerBase
{
    public FileArchivingWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 24 * 60 * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var settingProvider = workerContext.ServiceProvider.GetRequiredService<ISettingProvider>();
        var featureChecker = workerContext.ServiceProvider.GetRequiredService<IFeatureChecker>();
        var clock = workerContext.ServiceProvider.GetRequiredService<IClock>();
        var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<FileArchivingWorker>>();

        if (!await featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.Enable))
        {
            logger.LogDebug("File Manager module is disabled. Skipping archiving job.");
            return;
        }

        if (!await featureChecker.IsEnabledAsync(SufiAbpFileManagerFeatures.Archiving))
        {
            logger.LogDebug("File archiving feature is disabled. Skipping archiving job.");
            return;
        }

        var enabled = await settingProvider.GetAsync<bool>(FileArchivingSettings.Enabled);
        if (!enabled)
        {
            logger.LogDebug("File archiving is disabled. Skipping archiving job.");
            return;
        }

        var retentionDays = await settingProvider.GetAsync<int>(FileArchivingSettings.RetentionDays);
        var batchSize = await settingProvider.GetAsync<int>(FileArchivingSettings.BatchSize);
        var schedule = await settingProvider.GetOrNullAsync(FileArchivingSettings.Schedule);

        logger.LogInformation(
            "Starting scheduled file archiving. Retention: {RetentionDays} days, Batch size: {BatchSize}",
            retentionDays,
            batchSize);

        try
        {
            var backgroundJobManager = workerContext.ServiceProvider.GetRequiredService<Volo.Abp.BackgroundJobs.IBackgroundJobManager>();

            await backgroundJobManager.EnqueueAsync(new FileArchivingArgs
            {
                OlderThanDays = retentionDays,
                BatchSize = batchSize,
                ArchiveReason = "Automatic archiving - scheduled retention policy"
            });

            var archiveAIFiles = await settingProvider.GetAsync<bool>(FileArchivingSettings.ArchiveAIFiles);
            if (archiveAIFiles)
            {
                var aiRetentionDaysStr = await settingProvider.GetOrNullAsync(FileArchivingSettings.AIFilesRetentionDays);
                var aiRetentionDays = !string.IsNullOrEmpty(aiRetentionDaysStr)
                    ? int.Parse(aiRetentionDaysStr)
                    : retentionDays;

                if (aiRetentionDays != retentionDays)
                {
                    logger.LogInformation(
                        "Scheduling separate archiving job for AI files with retention: {AIRetentionDays} days",
                        aiRetentionDays);

                    await backgroundJobManager.EnqueueAsync(new FileArchivingArgs
                    {
                        OlderThanDays = aiRetentionDays,
                        BatchSize = batchSize,
                        StructureKey = FileStructureKeys.AI,
                        ArchiveReason = "Automatic archiving - AI files retention policy"
                    });
                }
            }

            logger.LogInformation("File archiving jobs scheduled successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to schedule file archiving jobs");
        }
        finally
        {
            Timer.Period = FileArchivingScheduleHelper.GetPeriodMilliseconds(schedule, clock.Now.ToUniversalTime());
        }
    }
}
