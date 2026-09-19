using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Tags.Relations;

[ExposeServices(typeof(ITagRelationMutationCoordinator), typeof(TagRelationCommandCoordinator))]
public class TagRelationCommandCoordinator : ITagRelationMutationCoordinator, ITransientDependency
{
    private readonly TagRelationCommandPolicy _policy;
    private readonly IRepository<TagRelationDefinition, Guid> _definitions;
    private readonly IEntityRelationRepository _relations;
    private readonly IRepository<TagRelationMutationReceipt, Guid> _receipts;
    private readonly ICurrentUser _user;
    private readonly ICurrentTenant _tenant;
    private readonly IClock _clock;

    public TagRelationCommandCoordinator(TagRelationCommandPolicy policy,
        IRepository<TagRelationDefinition, Guid> definitions, IEntityRelationRepository relations,
        IRepository<TagRelationMutationReceipt, Guid> receipts, ICurrentUser user, ICurrentTenant tenant, IClock clock)
    {
        _policy = policy;
        _definitions = definitions;
        _relations = relations;
        _receipts = receipts;
        _user = user;
        _tenant = tenant;
        _clock = clock;
    }

    public virtual async Task<TagRelationMutationReceiptRecord?> FindReceiptAsync(Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _receipts.FindAsync(requestId, cancellationToken: cancellationToken);
        return receipt?.ToRecord();
    }

    [UnitOfWork]
    public virtual async Task<TagRelationMutationReceiptRecord> ApplyAsync(TagRelationMutationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _policy.ValidateAsync(request, cancellationToken);
        var existing = await _receipts.FindAsync(request.RequestId, cancellationToken: cancellationToken);
        if (existing != null)
        {
            if (!existing.Matches(request)) throw new BusinessException(TagsErrorCodes.RelationReceiptConflict);
            return existing.ToRecord();
        }

        var recovered = await _relations.FindByRequestIdAsync(request.RequestId, _tenant.Id, cancellationToken);
        if (recovered != null)
        {
            var snapshot = recovered.Revisions.Single(x => x.RequestId == request.RequestId);
            if (snapshot.ApprovalDecisionId != request.ApprovalDecisionId)
                throw new BusinessException(TagsErrorCodes.RelationReceiptConflict);
            var recoveredReceipt = new TagRelationMutationReceipt(request, recovered.TenantId, snapshot.Sequence, UtcNow());
            await _receipts.InsertAsync(recoveredReceipt, autoSave: true, cancellationToken: cancellationToken);
            return recoveredReceipt.ToRecord();
        }

        var definition = await _definitions.GetAsync(request.DefinitionId, cancellationToken: cancellationToken);
        var actorId = _user.Id ?? throw Denied();
        var now = UtcNow();
        int version;
        if (request.Action == TagRelationMutationKind.Create)
        {
            var relation = new EntityRelation(request.RelationId, _tenant.Id, definition, request.Scope,
                request.Source, request.Target, now, request.ValidFromUtc, request.ValidToUtc, actorId,
                request.ApprovalDecisionId, request.RequestId, request.Reason);
            await _relations.InsertAsync(relation, autoSave: true, cancellationToken: cancellationToken);
            version = relation.Version;
        }
        else
        {
            var relation = await _relations.GetAsync(request.RelationId, cancellationToken: cancellationToken);
            version = request.Action == TagRelationMutationKind.Retract
                ? relation.Retract(request.ExpectedVersion, now, actorId, request.ApprovalDecisionId, request.RequestId, request.Reason)
                : relation.Revise(request.ExpectedVersion, now, request.ValidFromUtc, request.ValidToUtc, actorId,
                    request.ApprovalDecisionId, request.RequestId, request.Reason);
            await _relations.UpdateAsync(relation, autoSave: true, cancellationToken: cancellationToken);
        }

        var receipt = new TagRelationMutationReceipt(request, _tenant.Id, version, now);
        await _receipts.InsertAsync(receipt, autoSave: true, cancellationToken: cancellationToken);
        return receipt.ToRecord();
    }

    private DateTime UtcNow()
    {
        var now = _clock.Now;
        return now.Kind == DateTimeKind.Utc ? now : DateTime.SpecifyKind(now.ToUniversalTime(), DateTimeKind.Utc);
    }

    private static BusinessException Denied() => new(TagsErrorCodes.RelationCommandDenied);
}
