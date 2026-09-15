using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public abstract class CalendarEntityFrameworkCoreTestBase
    : CalendarEntityFrameworkCoreTestBase<CalendarEntityFrameworkCoreTestModule>
{
}

public abstract class CalendarEntityFrameworkCoreTestBase<TModule>
    : AbpIntegratedTest<TModule>, IAsyncLifetime
    where TModule : IAbpModule
{
    protected ICurrentTenant CurrentTenant => GetRequiredService<ICurrentTenant>();

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    public Task InitializeAsync() => WithUnitOfWorkAsync(async () =>
    {
        var db = await GetRequiredService<IDbContextProvider<CalendarDbContext>>().GetDbContextAsync();
        await db.Database.EnsureCreatedAsync();
    });

    // AbpIntegratedTest's IDisposable implementation shuts down the application and connection.
    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    protected async Task WithUnitOfWorkAsync(Func<Task> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action();
        await uow.CompleteAsync();
    }
}
