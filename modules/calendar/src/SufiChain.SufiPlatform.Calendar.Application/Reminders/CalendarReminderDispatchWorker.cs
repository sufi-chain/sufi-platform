using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

/// <summary>
/// Delivers due calendar reminders for the host and each active tenant.
/// </summary>
public class CalendarReminderDispatchWorker : AsyncPeriodicBackgroundWorkerBase
{
    public CalendarReminderDispatchWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = CalendarReminderSettings.DefaultDispatchIntervalSeconds * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        try
        {
            using var intervalScope = ServiceScopeFactory.CreateScope();
            await ApplyIntervalAsync(intervalScope.ServiceProvider);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CalendarReminderDispatchWorker could not read its interval.");
        }

        var total = 0;
        foreach (var tenantId in await ResolveTenantIdsAsync())
        {
            try
            {
                using var scope = ServiceScopeFactory.CreateScope();
                var provider = scope.ServiceProvider;
                var currentTenant = provider.GetRequiredService<ICurrentTenant>();
                using (currentTenant.Change(tenantId))
                {
                    total += await DispatchTenantAsync(provider);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "CalendarReminderDispatchWorker failed for tenant {TenantId}.", tenantId);
            }
        }

        if (total > 0)
        {
            Logger.LogInformation("CalendarReminderDispatchWorker dispatched {Count} reminders.", total);
        }
    }

    protected virtual async Task<int> DispatchTenantAsync(IServiceProvider serviceProvider)
    {
        var unitOfWorkManager = serviceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var dispatcher = serviceProvider.GetRequiredService<IEventReminderDispatcher>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var count = await dispatcher.DispatchDueAsync(clock.Now);
        await unitOfWork.CompleteAsync();
        return count;
    }

    private async Task ApplyIntervalAsync(IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<ISettingProvider>();
        var seconds = CalendarReminderSettings.ResolveIntervalSeconds(
            await settings.GetOrNullAsync(CalendarReminderSettings.DispatchIntervalSeconds));
        Timer.Period = seconds * 1000;
    }

    private async Task<List<Guid?>> ResolveTenantIdsAsync()
    {
        var ids = new List<Guid?> { null };
        using var scope = ServiceScopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetService<ITenantStore>();
        if (store is null)
        {
            return ids;
        }

        foreach (var tenant in await store.GetListAsync())
        {
            if (tenant.IsActive)
            {
                ids.Add(tenant.Id);
            }
        }

        return ids;
    }
}
