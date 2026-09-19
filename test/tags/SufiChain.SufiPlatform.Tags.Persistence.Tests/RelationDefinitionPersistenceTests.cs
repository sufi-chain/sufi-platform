using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Shouldly;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using SufiChain.SufiPlatform.Tags.MongoDB;
using Volo.Abp.Data;
using Volo.Abp;
using Volo.Abp.Testing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public abstract class RelationDefinitionPersistenceTests<TModule> : AbpIntegratedTest<TModule>
    where TModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        => options.UseAutofac();

    protected virtual Task InitializeStorageAsync() => Task.CompletedTask;
    protected abstract void AssertDuplicateException(Exception exception);

    [Fact]
    public async Task Should_RoundTrip_Versions_And_Filter_Tenants_In_New_Units_Of_Work()
    {
        await InitializeStorageAsync();
        var tenantId = Guid.NewGuid();
        var predicate = Guid.NewGuid();
        var original = new TagRelationDefinition(Guid.NewGuid(), tenantId, predicate,
            "supersedes-article", 1, "helpdesk.article", "helpdesk.article", requiresAcyclicGraph: true);
        var next = new TagRelationDefinition(Guid.NewGuid(), tenantId, predicate,
            "supersedes-article", 2, "helpdesk.article", "helpdesk.article", requiresAcyclicGraph: true);
        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await InUnitAsync(async repository =>
            {
                await repository.InsertAsync(original, autoSave: true);
                await repository.InsertAsync(next, autoSave: true);
            });
            await InUnitAsync(async repository =>
            {
                var loaded = await repository.GetAsync(original.Id);
                loaded.Revision.ShouldBe(1);
                loaded.RequiresAcyclicGraph.ShouldBeTrue();
                loaded.PredicateTagId.ShouldBe(predicate);
                loaded.TenantScopeKey.ShouldBe(tenantId.ToString("N"));
                loaded.SourceEntityType.ShouldBe("helpdesk.article");
                loaded.TargetEntityType.ShouldBe("helpdesk.article");
                (await repository.GetCountAsync()).ShouldBe(2);
            });
        }
        using (GetRequiredService<ICurrentTenant>().Change(null))
            await InUnitAsync(async repository => (await repository.FindAsync(original.Id)).ShouldBeNull());
        using (GetRequiredService<ICurrentTenant>().Change(Guid.NewGuid()))
            await InUnitAsync(async repository => (await repository.FindAsync(original.Id)).ShouldBeNull());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Enforce_Revision_Uniqueness_For_Host_And_Tenant(bool tenant)
    {
        await InitializeStorageAsync();
        Guid? tenantId = tenant ? Guid.NewGuid() : null;
        var predicate = Guid.NewGuid();
        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            await InUnitAsync(repository => repository.InsertAsync(NewDefinition(tenantId, predicate), autoSave: true));
            var exception = await Should.ThrowAsync<Exception>(() => InUnitAsync(repository =>
                repository.InsertAsync(NewDefinition(tenantId, predicate), autoSave: true)));
            AssertDuplicateException(exception);
            await InUnitAsync(async repository => (await repository.GetCountAsync()).ShouldBe(1));
        }
    }

    [Fact]
    public async Task Definition_Manager_Should_Require_A_Live_Predicate_In_The_Current_Tenant_And_Expected_Revision()
    {
        await InitializeStorageAsync();
        var predicate = Tags.Tag.CreateRelationPredicate(Guid.NewGuid(), "Related", "knowledge", "related-article");
        var classification = new Tags.Tag(Guid.NewGuid(), "Topic", "knowledge");
        var foreign = Tags.Tag.CreateRelationPredicate(Guid.NewGuid(), "Foreign", "knowledge", "foreign-predicate", Guid.NewGuid());
        await InUnitAsync(async _ =>
        {
            var tags = GetRequiredService<ITagRepository>();
            await tags.InsertAsync(predicate, autoSave: true);
            await tags.InsertAsync(classification, autoSave: true);
        });
        using (GetRequiredService<ICurrentTenant>().Change(foreign.TenantId))
            await InUnitAsync(_ => GetRequiredService<ITagRepository>().InsertAsync(foreign, autoSave: true));

        await InUnitAsync(async _ =>
        {
            var manager = GetRequiredService<TagRelationDefinitionManager>();
            var first = await manager.CreateRevisionAsync(predicate.Id, 0, "helpdesk.article", "helpdesk.article", symmetric: true);
            first.Revision.ShouldBe(1);
            var second = await manager.CreateRevisionAsync(predicate.Id, 1, "helpdesk.article", "helpdesk.article", symmetric: true);
            second.Revision.ShouldBe(2);
            (await Should.ThrowAsync<BusinessException>(() => manager.CreateRevisionAsync(predicate.Id, 0,
                "helpdesk.article", "helpdesk.article"))).Code.ShouldBe(TagsErrorCodes.RelationVersionConflict);
            (await Should.ThrowAsync<BusinessException>(() => manager.CreateRevisionAsync(classification.Id, 0,
                "helpdesk.article", "helpdesk.article"))).Code.ShouldBe(TagsErrorCodes.InvalidRelationDefinition);
            using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
                (await Should.ThrowAsync<BusinessException>(() => manager.CreateRevisionAsync(foreign.Id, 0,
                    "helpdesk.article", "helpdesk.article"))).Code.ShouldBe(TagsErrorCodes.InvalidRelationDefinition);

            var tags = GetRequiredService<ITagRepository>();
            (await tags.SearchAsync("knowledge", null)).Select(x => x.Id).ShouldBe(new[] { classification.Id });
            (await tags.GetListByScopeAsync("knowledge")).Select(x => x.Id).ShouldBe(new[] { classification.Id });
        });
    }

    [Fact]
    public async Task Should_Persist_History_And_Retry_Without_Changing_The_Original_Snapshot()
    {
        await InitializeStorageAsync();
        var relation = CreateRelation();
        var recorded = relation.Revisions.Single().RecordedAtUtc;
        var request = Guid.NewGuid();
        var actor = Guid.NewGuid();
        var approval = Guid.NewGuid();
        await InUnitAsync(_ => GetRequiredService<IRepository<EntityRelation, Guid>>().InsertAsync(relation, autoSave: true));
        await InUnitAsync(async _ =>
        {
            var repository = GetRequiredService<IRepository<EntityRelation, Guid>>();
            var loaded = await repository.GetAsync(relation.Id);
            loaded.Revise(1, recorded.AddSeconds(1), recorded.AddTicks(321), null, actor, approval, request, "Correct dates");
            await repository.UpdateAsync(loaded, autoSave: true);
        });
        await InUnitAsync(async _ =>
        {
            var repository = GetRequiredService<IRepository<EntityRelation, Guid>>();
            var loaded = await repository.GetAsync(relation.Id);
            loaded.Revise(1, recorded.AddSeconds(2), recorded.AddTicks(321), null, actor, approval, request, "Correct dates").ShouldBe(2);
            loaded.Revisions.Count.ShouldBe(2);
            loaded.Revisions.First().ValidFromUtc.ShouldBeNull();
            loaded.Revisions.Last().ApprovalDecisionId.ShouldBe(approval);
            loaded.Revisions.Last().ValidFromUtc!.Value.Kind.ShouldBe(DateTimeKind.Utc);
            loaded.Retract(2, recorded.AddSeconds(3), actor, approval, Guid.NewGuid(), "Withdrawn");
            await repository.UpdateAsync(loaded, autoSave: true);
        });
        await InUnitAsync(async _ =>
        {
            var loaded = await GetRequiredService<IRepository<EntityRelation, Guid>>().GetAsync(relation.Id);
            loaded.IsRetracted.ShouldBeTrue();
            loaded.Version.ShouldBe(3);
            loaded.Revisions.Select(x => x.Sequence).ShouldBe(new[] { 1, 2, 3 });
        });
        using (GetRequiredService<ICurrentTenant>().Change(Guid.NewGuid()))
            await InUnitAsync(async _ => (await GetRequiredService<IRepository<EntityRelation, Guid>>().FindAsync(relation.Id)).ShouldBeNull());
    }

    [Fact]
    public async Task Should_Reject_Concurrent_Revision_And_Keep_Only_Winning_History()
    {
        await InitializeStorageAsync();
        var original = CreateRelation();
        await InUnitAsync(_ => GetRequiredService<IRepository<EntityRelation, Guid>>().InsertAsync(original, autoSave: true));
        EntityRelation first = null!;
        EntityRelation second = null!;
        await InUnitAsync(async _ => first = await GetRequiredService<IRepository<EntityRelation, Guid>>().GetAsync(original.Id));
        await InUnitAsync(async _ => second = await GetRequiredService<IRepository<EntityRelation, Guid>>().GetAsync(original.Id));
        var recorded = original.Revisions.Single().RecordedAtUtc.AddSeconds(1);
        first.Revise(1, recorded, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Winning revision");
        second.Revise(1, recorded, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Losing revision");
        await InUnitAsync(_ => GetRequiredService<IRepository<EntityRelation, Guid>>().UpdateAsync(first, autoSave: true));
        await Should.ThrowAsync<AbpDbConcurrencyException>(() => InUnitAsync(_ =>
            GetRequiredService<IRepository<EntityRelation, Guid>>().UpdateAsync(second, autoSave: true)));
        await InUnitAsync(async _ =>
        {
            var loaded = await GetRequiredService<IRepository<EntityRelation, Guid>>().GetAsync(original.Id);
            loaded.Version.ShouldBe(2);
            loaded.Revisions.Count.ShouldBe(2);
            loaded.Revisions.Last().Reason.ShouldBe("Winning revision");
        });
    }

    [Fact]
    public async Task Should_Reject_Reverse_Duplicate_Of_Symmetric_Relation()
    {
        await InitializeStorageAsync();
        var definition = NewDefinition(null, Guid.NewGuid());
        var scope = new TagEntityReference("helpdesk.project", Guid.NewGuid());
        var a = new TagEntityReference("helpdesk.article", Guid.NewGuid());
        var b = new TagEntityReference("helpdesk.article", Guid.NewGuid());
        EntityRelation Create(TagEntityReference source, TagEntityReference target) => new(Guid.NewGuid(), null, definition,
            scope, source, target, new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc), null, null,
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Reviewed");
        await InUnitAsync(_ => GetRequiredService<IRepository<EntityRelation, Guid>>().InsertAsync(Create(a, b), autoSave: true));
        var exception = await Should.ThrowAsync<Exception>(() => InUnitAsync(_ =>
            GetRequiredService<IRepository<EntityRelation, Guid>>().InsertAsync(Create(b, a), autoSave: true)));
        AssertDuplicateException(exception);
    }

    [Fact]
    public async Task Vocabulary_Seeding_Should_Be_Repeatable_And_Tenant_Scoped()
    {
        await InitializeStorageAsync();
        var tenantId = Guid.NewGuid();
        await InUnitAsync(async _ =>
        {
            var seed = CreateSeeder();
            await seed.SeedAsync(new DataSeedContext());
            await seed.SeedAsync(new DataSeedContext());
            await seed.SeedAsync(new DataSeedContext(tenantId));
            await seed.SeedAsync(new DataSeedContext(tenantId));
            GetRequiredService<ICurrentTenant>().Id.ShouldBeNull();
            (await GetRequiredService<ITagRepository>().GetListAsync()).Single().Kind.ShouldBe(TagKind.RelationPredicate);
            (await GetRequiredService<IRepository<TagRelationDefinition, Guid>>().GetListAsync()).Single().Revision.ShouldBe(1);
            using (GetRequiredService<ICurrentTenant>().Change(tenantId))
                (await GetRequiredService<IRepository<TagRelationDefinition, Guid>>().GetListAsync()).Single().TenantId.ShouldBe(tenantId);
        });
    }

    [Fact]
    public async Task Vocabulary_Seeding_Should_Reject_Conflicting_Meaning_Without_Overwriting()
    {
        await InitializeStorageAsync();
        await InUnitAsync(async _ =>
        {
            await CreateSeeder().SeedAsync(new DataSeedContext());
            (await Should.ThrowAsync<BusinessException>(() => CreateSeeder(symmetric: false).SeedAsync(new DataSeedContext())))
                .Code.ShouldBe(TagsErrorCodes.InvalidRelationDefinition);
            (await GetRequiredService<IRepository<TagRelationDefinition, Guid>>().GetListAsync()).Single().IsSymmetric.ShouldBeTrue();
        });
    }

    [Fact]
    public async Task Vocabulary_Seeding_Should_Not_Convert_An_Existing_Classification_Tag()
    {
        await InitializeStorageAsync();
        await InUnitAsync(async _ =>
        {
            var tag = new Tags.Tag(Guid.NewGuid(), "related-item", "test.relations");
            await GetRequiredService<ITagRepository>().InsertAsync(tag, autoSave: true);
            await Should.ThrowAsync<BusinessException>(() => CreateSeeder().SeedAsync(new DataSeedContext()));
            tag.Kind.ShouldBe(TagKind.Classification);
            (await GetRequiredService<IRepository<TagRelationDefinition, Guid>>().GetCountAsync()).ShouldBe(0);
        });
    }

    private TagRelationVocabularyDataSeedContributor CreateSeeder(bool symmetric = true) => new(
        new[] { new TestVocabulary(symmetric) }, GetRequiredService<ITagRepository>(),
        GetRequiredService<IRepository<TagRelationDefinition, Guid>>(), GetRequiredService<TagRelationDefinitionManager>(),
        GetRequiredService<ICurrentTenant>(), GetRequiredService<IDataFilter>());

    private sealed class TestVocabulary(bool symmetric) : ITagRelationVocabularyContributor
    {
        public string Scope => "test.relations";
        public IReadOnlyList<TagRelationVocabularyEntry> GetEntries() => new[]
        {
            new TagRelationVocabularyEntry("related-item", "helpdesk.article", "helpdesk.article", isSymmetric: symmetric)
        };
    }

    private static EntityRelation CreateRelation() => new(Guid.NewGuid(), null, NewDefinition(null, Guid.NewGuid()),
        new TagEntityReference("helpdesk.project", Guid.NewGuid()), new TagEntityReference("helpdesk.article", Guid.NewGuid()),
        new TagEntityReference("helpdesk.article", Guid.NewGuid()), new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc),
        null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Reviewed");

    private static TagRelationDefinition NewDefinition(Guid? tenantId, Guid predicate) => new(
        Guid.NewGuid(), tenantId, predicate, "related-article", 1, "helpdesk.article", "helpdesk.article", isSymmetric: true);

    private async Task InUnitAsync(Func<IRepository<TagRelationDefinition, Guid>, Task> action)
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action(GetRequiredService<IRepository<TagRelationDefinition, Guid>>());
        await unit.CompleteAsync();
    }
}

public class RelationSqlitePersistenceTests : RelationDefinitionPersistenceTests<RelationSqliteTestModule>
{
    protected override void AssertDuplicateException(Exception exception)
    {
        var failure = exception.ShouldBeOfType<DbUpdateException>();
        failure.InnerException.ShouldBeOfType<Microsoft.Data.Sqlite.SqliteException>().SqliteErrorCode.ShouldBe(19);
    }

    protected override async Task InitializeStorageAsync()
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var context = await GetRequiredService<IDbContextProvider<TagsDbContext>>().GetDbContextAsync();
        await context.Database.EnsureCreatedAsync();
        await unit.CompleteAsync();
    }
}

public class RelationMongoPersistenceTests : RelationDefinitionPersistenceTests<RelationMongoTestModule>
{
    [Fact]
    public async Task Legacy_Tags_Without_Kind_Should_Remain_Searchable_Classification()
    {
        using var unit = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var repository = GetRequiredService<ITagRepository>();
        var tag = new Tags.Tag(Guid.NewGuid(), "Legacy", "legacy");
        await repository.InsertAsync(tag, autoSave: true);
        var context = await GetRequiredService<IMongoDbContextProvider<ITagsMongoDbContext>>().GetDbContextAsync();
        // Simulate a pre-upgrade document through real MongoDB, not a collection mock.
        await context.Tags.UpdateOneAsync(x => x.Id == tag.Id, Builders<Tags.Tag>.Update.Unset(x => x.Kind));
        (await repository.SearchAsync("legacy", null)).Single().Id.ShouldBe(tag.Id);
        (await repository.GetListByScopeAsync("legacy")).Single().Kind.ShouldBe(TagKind.Classification);
        (await repository.GetQueryableAsync()).Where(x => x.Kind != TagKind.RelationPredicate).Single().Id.ShouldBe(tag.Id);
        await unit.CompleteAsync();
    }

    protected override void AssertDuplicateException(Exception exception) =>
        exception.ShouldBeOfType<MongoWriteException>().WriteError.Category.ShouldBe(ServerErrorCategory.DuplicateKey);
}
