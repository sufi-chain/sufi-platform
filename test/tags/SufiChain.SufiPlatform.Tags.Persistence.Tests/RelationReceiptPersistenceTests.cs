using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shouldly;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public abstract class RelationReceiptPersistenceTests<TModule> : AbpIntegratedTest<TModule> where TModule : IAbpModule
{
    private static readonly DateTime Now = new(2026, 9, 16, 20, 0, 0, DateTimeKind.Utc);
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) => options.UseAutofac();
    protected virtual Task InitializeStorageAsync() => Task.CompletedTask;

    [Fact]
    public async Task Receipt_Should_RoundTrip_And_Keep_Request_Identity()
    {
        await InitializeStorageAsync();
        var tenantId = Guid.NewGuid();
            var request = NewRequest();
        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
            await GetRequiredService<IRepository<TagRelationMutationReceipt, Guid>>()
                .InsertAsync(new TagRelationMutationReceipt(request, tenantId, 1, Now), autoSave: true);
            await unit.CompleteAsync();
            using var read = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
            var loaded = await GetRequiredService<IRepository<TagRelationMutationReceipt, Guid>>().GetAsync(request.RequestId);
            loaded.Matches(request).ShouldBeTrue();
            loaded.AppliedVersion.ShouldBe(1);
            loaded.RecordedAtUtc.Kind.ShouldBe(DateTimeKind.Utc);
            loaded.ToRecord().RequestId.ShouldBe(request.RequestId);
            await read.CompleteAsync();
        }
    }

    private static TagRelationMutationRequest NewRequest() => new(Guid.NewGuid(), Guid.NewGuid(), 0,
        TagRelationMutationKind.Create, new("helpdesk.project", Guid.NewGuid()),
        new("helpdesk.article", Guid.NewGuid()), new("helpdesk.ticket", Guid.NewGuid()),
        null, null, Guid.NewGuid(), Guid.NewGuid(), "Human reviewed");
}

public class RelationReceiptSqliteTests : RelationReceiptPersistenceTests<RelationSqliteTestModule>
{
    protected override async Task InitializeStorageAsync()
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await (await GetRequiredService<IDbContextProvider<TagsDbContext>>().GetDbContextAsync()).Database.EnsureCreatedAsync();
        await unit.CompleteAsync();
    }
}

public class RelationReceiptMongoTests : RelationReceiptPersistenceTests<RelationMongoTestModule>
{
}
