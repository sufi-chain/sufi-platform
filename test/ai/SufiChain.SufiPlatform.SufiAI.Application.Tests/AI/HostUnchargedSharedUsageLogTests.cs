using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// AIC-18: the host charge worker must see a tenant shared-usage log.
/// The ordinary tenant filter hides it; <see cref="IAIUsageLogRepository.GetUnchargedSharedAllAsync"/> turns that filter off.
/// </summary>
public class HostUnchargedSharedUsageLogTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    [Fact]
    public async Task Host_query_returns_a_tenant_shared_usage_log_the_tenant_filter_hides()
    {
        var tenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var logId = Guid.NewGuid();
        var currentTenant = GetRequiredService<ICurrentTenant>();

        using (currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var log = new AIUsageLog(
                    logId,
                    Guid.NewGuid(),
                    AICapabilityType.ChatCompletion,
                    "gpt-test",
                    AIProviderType.OpenAI,
                    tenantId,
                    isSharedWorkspaceUsage: true);
                log.RecordSuccess(12, 8, 40, estimatedCost: 0.006m, isCostCalculated: true);
                await GetRequiredService<IAIUsageLogRepository>().InsertAsync(log, autoSave: true);
            });
        }

        using (currentTenant.Change(null))
        {
            var hidden = await WithUnitOfWorkAsync(() =>
                GetRequiredService<IAIUsageLogRepository>().GetListAsync());
            hidden.ShouldNotContain(log => log.Id == logId);

            var visible = await WithUnitOfWorkAsync(() =>
                GetRequiredService<IAIUsageLogRepository>().GetUnchargedSharedAllAsync(50));
            var match = visible.ShouldHaveSingleItem();
            match.Id.ShouldBe(logId);
            match.TenantId.ShouldBe(tenantId);
            match.EstimatedCost.ShouldBe(0.006m);
        }
    }
}
