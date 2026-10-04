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

    [Fact]
    public async Task Should_Show_Host_Only_The_Host_Workspace_When_Names_Match()
    {
        var tenantId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var tenantWorkspaceId = Guid.NewGuid();
        var name = "host-scope-" + Guid.NewGuid().ToString("N");

        await WithUnitOfWorkAsync(async () =>
        {
            await GetRequiredService<IWorkspaceRepository>().InsertAsync(
                new Workspace(hostId, name, AIProviderType.OpenAI, "gpt-4o-mini", tenantId: null),
                autoSave: true);
        });

        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await GetRequiredService<IWorkspaceRepository>().InsertAsync(
                    new Workspace(tenantWorkspaceId, name, AIProviderType.OpenAI, "gpt-4o-mini", tenantId),
                    autoSave: true);
            });
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IWorkspaceRepository>();
            var listed = await repository.GetListAsync(maxResultCount: 100);
            listed.ShouldAllBe(workspace => workspace.TenantId == null);
            listed.ShouldContain(workspace => workspace.Id == hostId);
            listed.ShouldNotContain(workspace => workspace.Id == tenantWorkspaceId);

            var byName = await repository.FindByNameAsync(name);
            byName.ShouldNotBeNull();
            byName.Id.ShouldBe(hostId);
            byName.TenantId.ShouldBeNull();
            (await repository.GetCountAsync()).ShouldBe(listed.Count);
        });

        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<IWorkspaceRepository>();
                var listed = await repository.GetListAsync(maxResultCount: 100);
                listed.ShouldAllBe(workspace => workspace.TenantId == tenantId);
                listed.ShouldContain(workspace => workspace.Id == tenantWorkspaceId);
                listed.ShouldNotContain(workspace => workspace.Id == hostId);

                var byName = await repository.FindByNameAsync(name);
                byName.ShouldNotBeNull();
                byName.Id.ShouldBe(tenantWorkspaceId);
            });
        }
    }

    [Fact]
    public async Task Should_Hide_Soft_Deleted_Host_Workspace_Until_The_Filter_Is_Disabled()
    {
        var hostId = Guid.NewGuid();
        var name = "host-deleted-" + Guid.NewGuid().ToString("N");

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IWorkspaceRepository>();
            await repository.InsertAsync(
                new Workspace(hostId, name, AIProviderType.OpenAI, "gpt-4o-mini", tenantId: null),
                autoSave: true);
            await repository.DeleteAsync(hostId, autoSave: true);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            (await GetRequiredService<IWorkspaceRepository>().FindByNameAsync(name)).ShouldBeNull();
        });

        using (GetRequiredService<IDataFilter>().Disable<ISoftDelete>())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var loaded = await GetRequiredService<IWorkspaceRepository>().FindByNameAsync(name);
                loaded.ShouldNotBeNull();
                loaded.Id.ShouldBe(hostId);
                loaded.IsDeleted.ShouldBeTrue();
            });
        }
    }
}
