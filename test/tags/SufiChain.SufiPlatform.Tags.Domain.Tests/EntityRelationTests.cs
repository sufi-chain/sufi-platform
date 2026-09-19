using Shouldly;
using SufiChain.SufiPlatform.Tags.Relations;
using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class EntityRelationTests
{
    private static readonly DateTime Recorded = new(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _actor = Guid.NewGuid();
    private readonly Guid _approval = Guid.NewGuid();

    [Fact]
    public void Ordinary_Tags_Should_Remain_Classification_And_Predicates_Should_Reject_Assignment()
    {
        var ordinary = new Tag(Guid.NewGuid(), "Login", "support");
        ordinary.Kind.ShouldBe(TagKind.Classification);
        ordinary.StableKey.ShouldBeNull();
        ordinary.EnsureClassification();
        var predicate = Tag.CreateRelationPredicate(Guid.NewGuid(), "Related", "support", "related-article");
        predicate.Kind.ShouldBe(TagKind.RelationPredicate);
        Should.Throw<BusinessException>(() => predicate.EnsureClassification()).Code
            .ShouldBe(TagsErrorCodes.PredicateRequiresRelationWorkflow);
    }

    [Fact]
    public void Revisions_Should_Preserve_Original_And_Retraction_Should_Be_Terminal()
    {
        var relation = Create();
        relation.Revise(1, Recorded.AddSeconds(1), Recorded, Recorded.AddDays(1), _actor, _approval, Guid.NewGuid(), "Correct dates");
        relation.Retract(2, Recorded.AddSeconds(2), _actor, _approval, Guid.NewGuid(), "Evidence withdrawn");
        relation.Version.ShouldBe(3);
        relation.IsRetracted.ShouldBeTrue();
        relation.Revisions.First().ValidFromUtc.ShouldBeNull();
        relation.Revisions.Last().ValidFromUtc.ShouldBe(Recorded);
        relation.Revisions.Select(x => x.Action).ShouldBe(new[] {
            RelationRevisionAction.Accept, RelationRevisionAction.Revise, RelationRevisionAction.Retract });
        Should.Throw<NotSupportedException>(() => ((ICollection<EntityRelationRevision>)relation.Revisions).Clear());
        Should.Throw<BusinessException>(() => relation.Revise(3, Recorded.AddSeconds(3), null, null,
            _actor, _approval, Guid.NewGuid(), "Reopen")).Code.ShouldBe(TagsErrorCodes.RelationRetracted);
    }

    [Fact]
    public void Exact_Retry_Should_Return_Original_Sequence_Even_After_Subsequent_Changes()
    {
        var relation = Create();
        var request = Guid.NewGuid();
        relation.Revise(1, Recorded.AddSeconds(1), Recorded.AddTicks(123), null, _actor, _approval, request, "Reviewed");
        relation.Retract(2, Recorded.AddSeconds(2), _actor, _approval, Guid.NewGuid(), "Withdrawn");
        relation.Revise(1, Recorded.AddSeconds(3), Recorded.AddTicks(123), null, _actor, _approval, request, "Reviewed").ShouldBe(2);
        relation.Version.ShouldBe(3);
        relation.Revisions.Count.ShouldBe(3);
        Should.Throw<BusinessException>(() => relation.Revise(1, Recorded.AddSeconds(3), null, null,
            _actor, _approval, request, "Changed payload")).Code.ShouldBe(TagsErrorCodes.RelationRequestConflict);
    }

    [Fact]
    public void Should_Reject_Stale_Version_Backdated_Record_And_Invalid_Interval_Without_Changing_History()
    {
        var relation = Create();
        Should.Throw<BusinessException>(() => relation.Revise(0, Recorded, null, null,
            _actor, _approval, Guid.NewGuid(), "Stale")).Code.ShouldBe(TagsErrorCodes.RelationVersionConflict);
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded.AddSeconds(-1), null, null,
            _actor, _approval, Guid.NewGuid(), "Backdated"));
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded, Recorded, Recorded,
            _actor, _approval, Guid.NewGuid(), "Invalid interval"));
        Should.Throw<ArgumentException>(() => relation.Revise(1, DateTime.SpecifyKind(Recorded, DateTimeKind.Unspecified), null, null,
            _actor, _approval, Guid.NewGuid(), "Unspecified"));
        relation.Version.ShouldBe(1);
        relation.Revisions.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Require_Actor_Approval_Request_And_Reason()
    {
        var relation = Create();
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded, null, null, Guid.Empty, _approval, Guid.NewGuid(), "Review"));
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded, null, null, _actor, Guid.Empty, Guid.NewGuid(), "Review"));
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded, null, null, _actor, _approval, Guid.Empty, "Review"));
        Should.Throw<ArgumentException>(() => relation.Revise(1, Recorded, null, null, _actor, _approval, Guid.NewGuid(), " "));
        relation.Version.ShouldBe(1);
    }

    [Fact]
    public void Should_Bound_Embedded_History_Without_Discarding_Evidence()
    {
        var relation = Create();
        for (var version = 1; version < EntityRelation.MaxRevisions - 1; version++)
            relation.Revise(version, Recorded.AddSeconds(version), null, null, _actor, _approval, Guid.NewGuid(), "Review");
        Should.Throw<BusinessException>(() => relation.Revise(EntityRelation.MaxRevisions - 1, Recorded.AddDays(1),
            null, null, _actor, _approval, Guid.NewGuid(), "Overflow")).Code.ShouldBe(TagsErrorCodes.RelationHistoryLimit);
        relation.Retract(EntityRelation.MaxRevisions - 1, Recorded.AddDays(1), _actor, _approval, Guid.NewGuid(), "Withdraw at capacity");
        relation.Revisions.Count.ShouldBe(EntityRelation.MaxRevisions);
    }

    private EntityRelation Create() => new(Guid.NewGuid(), null,
        new TagRelationDefinition(Guid.NewGuid(), null, Guid.NewGuid(), "related-article", 1,
            "helpdesk.article", "helpdesk.article", isSymmetric: true),
        new TagEntityReference("helpdesk.project", Guid.NewGuid()),
        new TagEntityReference("helpdesk.article", Guid.NewGuid()), new TagEntityReference("helpdesk.article", Guid.NewGuid()),
        Recorded, null, null, _actor, _approval, Guid.NewGuid(), "Human reviewed");
}
