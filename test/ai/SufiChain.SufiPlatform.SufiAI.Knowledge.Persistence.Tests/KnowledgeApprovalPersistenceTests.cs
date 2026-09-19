using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Knowledge;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public abstract class KnowledgeApprovalPersistenceTests<TModule> : AbpIntegratedTest<TModule> where TModule : IAbpModule
{
    protected static readonly DateTime Now = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) => options.UseAutofac();
    protected virtual Task InitializeStorageAsync() => Task.CompletedTask;
    protected abstract void AssertDuplicate(Exception exception);

    private static TagRelationMutationRequest Request(Guid? requestId = null) => new(Guid.NewGuid(), Guid.NewGuid(), 0,
        TagRelationMutationKind.Create, new("helpdesk.project", Guid.NewGuid()),
        new("helpdesk.article", Guid.NewGuid()), new("helpdesk.ticket", Guid.NewGuid()),
        Now.AddTicks(123), null, Guid.NewGuid(), requestId ?? Guid.NewGuid(), "Documented resolution");
    private static KnowledgeRelationProposal New(TagRelationMutationRequest request, Guid? tenantId = null,
        Guid? proposalId = null, int version = 1) => new(proposalId ?? Guid.NewGuid(), version, tenantId,
        request, request.Source, "published-v3", new string('a', 64), "utf16:0:64", Guid.NewGuid(), Now);

    [Fact]
    public async Task Should_Preserve_Exact_Approval_And_Revocation_Across_Units_Of_Work()
    {
        await InitializeStorageAsync();
        var tenantId = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var request = Request();
        var proposal = New(request, tenantId);
        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await InUnitAsync(r => r.InsertAsync(proposal, autoSave: true));
            await InUnitAsync(async r =>
            {
                var loaded = await r.GetAsync(proposal.Id);
                loaded.Approve(1, reviewer, Now, Now.AddHours(1), "Checked published evidence");
                await r.UpdateAsync(loaded, autoSave: true);
            });
            await InUnitAsync(async r =>
            {
                var loaded = await r.FindForVerificationAsync(proposal.Id, tenantId);
                loaded.ShouldNotBeNull();
                loaded.Authorizes(request, tenantId, reviewer, Now).ShouldBeTrue();
                loaded.ValidFromUtcTicks.ShouldBe(request.ValidFromUtc!.Value.Ticks);
                loaded.EvidenceSha256.ShouldBe(new string('a', 64));
                loaded.ReviewedAtUtc!.Value.Kind.ShouldBe(DateTimeKind.Utc);
                loaded.Revoke(2, reviewer, Now.AddMinutes(1), "Evidence withdrawn");
                await r.UpdateAsync(loaded, autoSave: true);
            });
            await InUnitAsync(async r =>
            {
                var loaded = await r.GetAsync(proposal.Id);
                loaded.Authorizes(request, tenantId, reviewer, Now.AddMinutes(2)).ShouldBeFalse();
                loaded.ReviewReason.ShouldBe("Checked published evidence");
                loaded.RevocationReason.ShouldBe("Evidence withdrawn");
                loaded.Version.ShouldBe(3);
            });
        }
        await InUnitAsync(async r => (await r.FindAsync(proposal.Id)).ShouldBeNull());
        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
            await InUnitAsync(async r => (await r.FindForVerificationAsync(proposal.Id, null)).ShouldBeNull());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Should_Reject_Duplicate_Version_Or_Request_For_Host_And_Tenant(bool tenant, bool duplicateRequest)
    {
        await InitializeStorageAsync();
        Guid? tenantId = tenant ? Guid.NewGuid() : null;
        using var scope = GetRequiredService<ICurrentTenant>().Change(tenantId);
        var request = Request();
        var original = New(request, tenantId);
        var duplicate = New(Request(duplicateRequest ? request.RequestId : null), tenantId,
            duplicateRequest ? Guid.NewGuid() : original.ProposalId);
        await InUnitAsync(r => r.InsertAsync(original, autoSave: true));
        var exception = await Should.ThrowAsync<Exception>(() => InUnitAsync(r => r.InsertAsync(duplicate, autoSave: true)));
        AssertDuplicate(exception);
        await InUnitAsync(async r => (await r.GetCountAsync()).ShouldBe(1));
    }

    [Fact]
    public async Task Should_Detect_A_Later_Version_Without_Crossing_Tenants()
    {
        await InitializeStorageAsync();
        var original = New(Request());
        await InUnitAsync(async r =>
        {
            await r.InsertAsync(original, autoSave: true);
            await r.InsertAsync(New(Request(), proposalId: original.ProposalId, version: 2), autoSave: true);
            (await r.HasLaterVersionAsync(null, original.ProposalId, 1)).ShouldBeTrue();
            (await r.HasLaterVersionAsync(null, original.ProposalId, 2)).ShouldBeFalse();
            using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
                (await r.HasLaterVersionAsync(Guid.NewGuid(), original.ProposalId, 1)).ShouldBeFalse();
        });
    }

    [Fact]
    public async Task Competing_Reviews_Must_Not_Overwrite_The_Winning_Decision()
    {
        await InitializeStorageAsync();
        var proposal = New(Request());
        await InUnitAsync(r => r.InsertAsync(proposal, autoSave: true));
        KnowledgeRelationProposal first = null!, second = null!;
        await InUnitAsync(async r => first = await r.GetAsync(proposal.Id));
        await InUnitAsync(async r => second = await r.GetAsync(proposal.Id));
        first.Approve(1, Guid.NewGuid(), Now, Now.AddHours(1), "Approved");
        second.Reject(1, Guid.NewGuid(), Now, "Competing rejection");
        await InUnitAsync(r => r.UpdateAsync(first, autoSave: true));
        await Should.ThrowAsync<AbpDbConcurrencyException>(() => InUnitAsync(r => r.UpdateAsync(second, autoSave: true)));
        await InUnitAsync(async r => (await r.GetAsync(proposal.Id)).State.ShouldBe(KnowledgeProposalState.Approved));
    }

    [Fact]
    public async Task Intent_Should_RoundTrip_Pending_Apply_And_Applied_Receipt()
    {
        await InitializeStorageAsync();
        var tenantId = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var request = Request();
        var proposal = New(request, tenantId);
        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await InUnitAsync(r => r.InsertAsync(proposal, autoSave: true));
            await InUnitAsync(async r =>
            {
                var loaded = await r.GetAsync(proposal.Id);
                loaded.Approve(1, reviewer, Now, Now.AddHours(1), "Checked published evidence");
                await r.UpdateAsync(loaded, autoSave: true);
            });
            using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
            var intents = GetRequiredService<IKnowledgeRelationApplicationIntentRepository>();
            var loaded = await GetRequiredService<IKnowledgeRelationProposalRepository>().GetAsync(proposal.Id);
            var intent = new KnowledgeRelationApplicationIntent(loaded, Now);
            await intents.InsertAsync(intent, autoSave: true);
            await unit.CompleteAsync();
            using var read = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
            var persisted = await GetRequiredService<IKnowledgeRelationApplicationIntentRepository>().GetAsync(request.RequestId);
            persisted.State.ShouldBe(KnowledgeApplicationIntentState.PendingApply);
            persisted.RecordApplied(new TagRelationMutationReceiptRecord(request.RequestId, request.RelationId, 1,
                request.Action, request.ApprovalDecisionId, Now.AddMinutes(1)), Now.AddMinutes(1));
            await GetRequiredService<IKnowledgeRelationApplicationIntentRepository>().UpdateAsync(persisted, autoSave: true);
            await read.CompleteAsync();
            using var verify = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
            var applied = await GetRequiredService<IKnowledgeRelationApplicationIntentRepository>().GetAsync(request.RequestId);
            applied.State.ShouldBe(KnowledgeApplicationIntentState.Applied);
            applied.AppliedRelationVersion.ShouldBe(1);
            applied.CreatedAtUtc.Kind.ShouldBe(DateTimeKind.Utc);
            await verify.CompleteAsync();
        }
    }

    private async Task InUnitAsync(Func<IKnowledgeRelationProposalRepository, Task> action)
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action(GetRequiredService<IKnowledgeRelationProposalRepository>());
        await unit.CompleteAsync();
    }
}

public class KnowledgeSqlitePersistenceTests : KnowledgeApprovalPersistenceTests<KnowledgeSqliteTestModule>
{
    protected override void AssertDuplicate(Exception exception) =>
        exception.ShouldBeOfType<DbUpdateException>().InnerException.ShouldBeOfType<Microsoft.Data.Sqlite.SqliteException>()
            .SqliteErrorCode.ShouldBe(19);
    protected override async Task InitializeStorageAsync()
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var context = await GetRequiredService<IDbContextProvider<AIDbContext>>().GetDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        await unit.CompleteAsync();
    }
}

public class KnowledgeMongoPersistenceTests : KnowledgeApprovalPersistenceTests<KnowledgeMongoTestModule>
{
    protected override void AssertDuplicate(Exception exception) =>
        exception.ShouldBeOfType<MongoWriteException>().WriteError.Category.ShouldBe(ServerErrorCategory.DuplicateKey);
}
