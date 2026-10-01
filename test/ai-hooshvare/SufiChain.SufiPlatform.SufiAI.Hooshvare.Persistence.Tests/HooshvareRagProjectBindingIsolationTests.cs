using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public abstract class HooshvareRagProjectBindingIsolationTests<TModule> : AbpIntegratedTest<TModule>
    where TModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) =>
        options.UseAutofac();

    [Fact]
    public async Task Should_Hide_A_Binding_From_Another_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var binding = new HooshvareRagProjectBinding(
            Guid.NewGuid(),
            tenantA,
            Guid.NewGuid(),
            Guid.NewGuid(),
            HooshvareRagSourceNames.HelpDeskKnowledgeBase);

        using (GetRequiredService<ICurrentTenant>().Change(tenantA))
        {
            await InUnitAsync(repository => repository.InsertAsync(binding, autoSave: true));
            await InUnitAsync(async repository =>
            {
                var loaded = await repository.GetAsync(binding.Id);
                loaded.IsEnabled.ShouldBeTrue();
                loaded.SourceName.ShouldBe(HooshvareRagSourceNames.HelpDeskKnowledgeBase);
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(tenantB))
        {
            await InUnitAsync(async repository =>
                (await repository.FindAsync(binding.Id)).ShouldBeNull());
        }

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await InUnitAsync(async repository =>
                (await repository.FindAsync(binding.Id)).ShouldNotBeNull());
        }
    }

    private async Task InUnitAsync(Func<IHooshvareRagProjectBindingRepository, Task> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action(GetRequiredService<IHooshvareRagProjectBindingRepository>());
        await uow.CompleteAsync();
    }
}
