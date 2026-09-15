using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

public abstract class CalendarMongoDBTestBase : AbpIntegratedTest<CalendarMongoDBTestModule>
{
    protected ICurrentTenant CurrentTenant => GetRequiredService<ICurrentTenant>();

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected async Task WithUnitOfWorkAsync(Func<Task> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action();
        await uow.CompleteAsync();
    }
}
