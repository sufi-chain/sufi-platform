using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceTenantIsolationTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    [Fact]
    public async Task Should_Hide_Workspace_From_Another_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        using (GetRequiredService<ICurrentTenant>().Change(tenantA))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<IWorkspaceRepository>();
                await repository.InsertAsync(
                    new Workspace(workspaceId, "tenant-a-workspace", AIProviderType.OpenAI, "gpt-4o-mini", tenantA),
                    autoSave: true);
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(tenantB))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<IWorkspaceRepository>();
                (await repository.FindAsync(workspaceId)).ShouldBeNull();
            });
        }

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var loaded = await GetRequiredService<IWorkspaceRepository>().GetAsync(workspaceId);
                loaded.TenantId.ShouldBe(tenantA);
                loaded.Name.ShouldBe("tenant-a-workspace");
            });
        }
    }
}
