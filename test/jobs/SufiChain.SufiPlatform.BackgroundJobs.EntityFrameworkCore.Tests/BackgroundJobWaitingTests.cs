using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.BackgroundJobs;

public class BackgroundJobWaitingTests : AbpIntegratedTest<BackgroundJobsSqliteTestModule>
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) =>
        options.UseAutofac();

    [Fact]
    public async Task Waiting_List_Should_Return_Due_Jobs_For_The_Application_Only()
    {
        var due = new BackgroundJobRecord(Guid.NewGuid())
        {
            ApplicationName = "Sufi",
            JobName = "Index",
            JobArgs = "{}",
            IsAbandoned = false,
            NextTryTime = DateTime.UtcNow.AddMinutes(-5)
        };
        var otherApp = new BackgroundJobRecord(Guid.NewGuid())
        {
            ApplicationName = "Other",
            JobName = "Index",
            JobArgs = "{}",
            IsAbandoned = false,
            NextTryTime = DateTime.UtcNow.AddMinutes(-5)
        };

        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var repository = GetRequiredService<IBackgroundJobRepository>();
        await repository.InsertAsync(due, autoSave: true);
        await repository.InsertAsync(otherApp, autoSave: true);
        await uow.CompleteAsync();

        using var read = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var waiting = await GetRequiredService<IBackgroundJobRepository>().GetWaitingListAsync("Sufi", 10);
        await read.CompleteAsync();

        waiting.Select(job => job.Id).ShouldBe([due.Id]);
    }
}
