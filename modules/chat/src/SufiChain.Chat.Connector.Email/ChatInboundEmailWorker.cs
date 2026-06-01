using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.Chat.Connectors.Email.Settings;
using SufiChain.Chat.Features;
using SufiChain.SufiAbp.BackgroundWorkers;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.TenantManagement;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace SufiChain.Chat.Connectors.Email;

public class ChatInboundEmailWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ChatInboundEmailWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 1000 * 60;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var featureChecker = workerContext.ServiceProvider.GetRequiredService<IFeatureChecker>();
        if (!await featureChecker.IsEnabledAsync(ChatFeatures.EmailConnector))
        {
            return;
        }

        var tenantRepository = workerContext.ServiceProvider.GetRequiredService<ITenantRepository>();
        var tenants = await tenantRepository.GetListAsync(includeDetails: false);

        foreach (var tenant in tenants)
        {
            await ProcessTenantAsync(workerContext, tenant.Id);
        }

        await ProcessTenantAsync(workerContext, tenantId: null);
    }

    protected virtual async Task ProcessTenantAsync(PeriodicBackgroundWorkerContext workerContext, Guid? tenantId)
    {
        using var scope = ServiceScopeFactory.CreateScope();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        using var tenantChange = currentTenant.Change(tenantId);

        try
        {
            var settingsReader = scope.ServiceProvider.GetRequiredService<IChatEmailConnectorSettingsReader>();
            var settings = await settingsReader.GetAsync();
            if (!settings.IsInboundConfigured)
            {
                return;
            }

            var processor = scope.ServiceProvider.GetRequiredService<ChatInboundEmailProcessor>();
            await processor.ProcessAsync(settings, tenantId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Chat inbound email worker failed for tenant {TenantId}", tenantId);
        }
    }
}
