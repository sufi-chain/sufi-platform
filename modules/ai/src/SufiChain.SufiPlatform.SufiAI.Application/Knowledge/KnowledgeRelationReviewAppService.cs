using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Permissions;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

/// <summary>
/// Human-only proposal and review workflow. Reviewer identity comes from the
/// authenticated context. This service is not an MCP tool and is not remoted.
/// </summary>
[RemoteService(IsEnabled = false)]
public class KnowledgeRelationReviewAppService : SufiApplicationService, IKnowledgeRelationReviewAppService
{
    private readonly IKnowledgeRelationProposalRepository _proposals;
    private readonly IKnowledgeRelationApplicationIntentRepository _intents;
    private readonly IPermissionChecker _permissions;
    private readonly IFeatureChecker _features;
    private readonly ICurrentUser _user;
    private readonly ICurrentTenant _tenant;
    private readonly IGuidGenerator _ids;
    private readonly IClock _clock;
    private readonly IKnowledgeProposalEvidenceValidator[] _evidence;
    private readonly ITagRelationEndpointAccess[] _endpoints;
    private readonly ITagRelationMutationCoordinator[] _commands;

    public KnowledgeRelationReviewAppService(IKnowledgeRelationProposalRepository proposals,
        IKnowledgeRelationApplicationIntentRepository intents, IPermissionChecker permissions, IFeatureChecker features,
        ICurrentUser user, ICurrentTenant tenant, IGuidGenerator ids, IClock clock,
        IEnumerable<IKnowledgeProposalEvidenceValidator> evidence, IEnumerable<ITagRelationEndpointAccess> endpoints,
        IEnumerable<ITagRelationMutationCoordinator> commands)
    {
        _proposals = proposals;
        _intents = intents;
        _permissions = permissions;
        _features = features;
        _user = user;
        _tenant = tenant;
        _ids = ids;
        _clock = clock;
        _evidence = evidence.ToArray();
        _endpoints = endpoints.ToArray();
        _commands = commands.ToArray();
    }

    public virtual async Task<KnowledgeRelationProposalDto> CreateAsync(CreateKnowledgeRelationProposalDto input)
    {
        var actorId = await RequireAsync(TagsPermissions.Relations.Default);
        ArgumentNullException.ThrowIfNull(input);
        var request = new TagRelationMutationRequest(input.RelationId, input.DefinitionId, input.ExpectedRelationVersion,
            input.Action, new TagEntityReference(input.ScopeType, input.ScopeId),
            new TagEntityReference(input.SourceType, input.SourceId), new TagEntityReference(input.TargetType, input.TargetId),
            input.ValidFromUtc, input.ValidToUtc, _ids.Create(), _ids.Create(), input.Reason);
        var evidenceRef = new TagEntityReference(input.EvidenceType, input.EvidenceId);
        var evidence = new KnowledgeProposalEvidence(_tenant.Id, request.Scope.EntityType, request.Scope.EntityId,
            evidenceRef.EntityType, evidenceRef.EntityId, input.EvidenceVersion, input.EvidenceSha256, input.EvidenceLocator);
        if (_evidence.Length != 1 || _endpoints.Length != 1 ||
            !await _evidence[0].IsCurrentAndAccessibleAsync(evidence, actorId) ||
            !await _endpoints[0].CanRelateInScopeAsync(request.Source, request.Target, request.Scope))
            throw Denied();

        var proposalId = input.ProposalId is { } existing && existing != Guid.Empty ? existing : _ids.Create();
        var latest = await _proposals.FindLatestAsync(proposalId, _tenant.Id);
        var version = 1;
        if (latest != null)
        {
            if (latest.TenantId != _tenant.Id) throw Denied();
            version = latest.ProposalVersion + 1;
        }

        var proposal = new KnowledgeRelationProposal(proposalId, version, _tenant.Id, request, evidenceRef,
            input.EvidenceVersion, input.EvidenceSha256, input.EvidenceLocator, actorId, UtcNow());
        await _proposals.InsertAsync(proposal, autoSave: true);
        return await ToDtoAsync(proposal);
    }

    public virtual Task<KnowledgeRelationProposalDto> ApproveAsync(ReviewKnowledgeRelationDto input) =>
        ReviewAsync(input, async (proposal, actorId, now) =>
        {
            if (proposal.ProposedBy == actorId) throw Denied();
            if (await _proposals.HasLaterVersionAsync(_tenant.Id, proposal.ProposalId, proposal.ProposalVersion) ||
                !await CurrentEvidenceAsync(proposal, actorId) ||
                !await CurrentEndpointsAsync(proposal.ToMutationRequest()))
                throw Denied();
            if (!input.ExpiresAtUtc.HasValue) throw new BusinessException(AIErrorCodes.KnowledgeReviewInvalid);
            proposal.Approve(input.ExpectedVersion, actorId, now, input.ExpiresAtUtc.Value, input.Reason);
            await _proposals.UpdateAsync(proposal, autoSave: true);
            await _intents.InsertAsync(new KnowledgeRelationApplicationIntent(proposal, now), autoSave: true);
        });

    public virtual Task<KnowledgeRelationProposalDto> RejectAsync(ReviewKnowledgeRelationDto input) =>
        ReviewAsync(input, (proposal, actorId, now) =>
        {
            if (proposal.ProposedBy == actorId) throw Denied();
            proposal.Reject(input.ExpectedVersion, actorId, now, input.Reason);
            return _proposals.UpdateAsync(proposal, autoSave: true);
        });

    public virtual Task<KnowledgeRelationProposalDto> RevokeAsync(ReviewKnowledgeRelationDto input) =>
        ReviewAsync(input, async (proposal, actorId, now) =>
        {
            proposal.Revoke(input.ExpectedVersion, actorId, now, input.Reason);
            await _proposals.UpdateAsync(proposal, autoSave: true);
            var intent = await _intents.FindAsync(proposal.RequestId);
            if (intent != null && intent.State == KnowledgeApplicationIntentState.PendingApply)
            {
                intent.MarkNeedsReview("Approval revoked before application.", now);
                await _intents.UpdateAsync(intent, autoSave: true);
            }
        });

    [UnitOfWork]
    public virtual async Task<KnowledgeRelationProposalDto> TryApplyAsync(Guid requestId)
    {
        await RequireAsync(TagsPermissions.Relations.Mutate);
        var intent = await _intents.FindAsync(requestId) ?? throw Denied();
        if (intent.TenantId != _tenant.Id) throw Denied();
        var now = UtcNow();
        if (intent.State == KnowledgeApplicationIntentState.Applied) return await LoadDtoAsync(intent);

        var receipt = _commands.Length == 1 ? await _commands[0].FindReceiptAsync(requestId) : null;
        if (receipt != null)
        {
            intent.RecordApplied(receipt, now);
            await _intents.UpdateAsync(intent, autoSave: true);
            return await LoadDtoAsync(intent);
        }

        var proposal = await _proposals.FindForVerificationAsync(intent.ApprovalDecisionId, _tenant.Id);
        var request = intent.ToMutationRequest();
        if (proposal == null || !proposal.Authorizes(request, _tenant.Id, intent.ReviewerId, now) ||
            await _proposals.HasLaterVersionAsync(_tenant.Id, intent.ProposalId, intent.ProposalVersion) ||
            !await CurrentEvidenceAsync(proposal, intent.ReviewerId) || !await CurrentEndpointsAsync(request) ||
            _commands.Length != 1)
        {
            intent.MarkNeedsReview("Approval, evidence or access is no longer valid.", now);
            await _intents.UpdateAsync(intent, autoSave: true);
            return await LoadDtoAsync(intent);
        }

        try
        {
            receipt = await _commands[0].ApplyAsync(request);
        }
        catch (BusinessException)
        {
            intent.MarkNeedsReview("Tags application denied or raced with a later change.", now);
            await _intents.UpdateAsync(intent, autoSave: true);
            return await LoadDtoAsync(intent);
        }

        intent.RecordApplied(receipt, now);
        await _intents.UpdateAsync(intent, autoSave: true);
        return await LoadDtoAsync(intent);
    }

    private async Task<KnowledgeRelationProposalDto> ReviewAsync(ReviewKnowledgeRelationDto input,
        Func<KnowledgeRelationProposal, Guid, DateTime, Task> action)
    {
        ArgumentNullException.ThrowIfNull(input);
        var actorId = await RequireAsync(TagsPermissions.Relations.Review);
        var proposal = await _proposals.FindAsync(input.DecisionId) ?? throw Denied();
        if (proposal.TenantId != _tenant.Id) throw Denied();
        await action(proposal, actorId, UtcNow());
        return await ToDtoAsync(proposal);
    }

    private async Task<Guid> RequireAsync(string permission)
    {
        var actorId = _user.Id;
        if (!_user.IsAuthenticated || !actorId.HasValue || actorId == Guid.Empty ||
            !await _features.IsEnabledAsync(SufiAIFeatures.Enable) ||
            !await _features.IsEnabledAsync(SufiTagsFeatures.Enable) ||
            !await _features.IsEnabledAsync(SufiTagsFeatures.Relations) ||
            !await _permissions.IsGrantedAsync(TagsPermissions.Relations.Default) ||
            !await _permissions.IsGrantedAsync(permission))
            throw Denied();
        return actorId.Value;
    }

    private async Task<bool> CurrentEvidenceAsync(KnowledgeRelationProposal proposal, Guid actorId) =>
        _evidence.Length == 1 && await _evidence[0].IsCurrentAndAccessibleAsync(proposal.ToEvidence(), actorId);

    private async Task<bool> CurrentEndpointsAsync(TagRelationMutationRequest request) =>
        _endpoints.Length == 1 && await _endpoints[0].CanRelateInScopeAsync(request.Source, request.Target, request.Scope);

    private async Task<KnowledgeRelationProposalDto> ToDtoAsync(KnowledgeRelationProposal proposal)
    {
        var intent = await _intents.FindAsync(proposal.RequestId);
        return new KnowledgeRelationProposalDto
        {
            DecisionId = proposal.Id,
            ProposalId = proposal.ProposalId,
            ProposalVersion = proposal.ProposalVersion,
            ReviewVersion = proposal.Version,
            State = proposal.State,
            RequestId = proposal.RequestId,
            ReviewerId = proposal.ReviewerId,
            ApplicationState = intent?.State,
            AppliedRelationVersion = intent?.AppliedRelationVersion
        };
    }

    private async Task<KnowledgeRelationProposalDto> LoadDtoAsync(KnowledgeRelationApplicationIntent intent)
    {
        var proposal = await _proposals.FindAsync(intent.ApprovalDecisionId) ?? throw Denied();
        return await ToDtoAsync(proposal);
    }

    private DateTime UtcNow()
    {
        var now = _clock.Now;
        return now.Kind == DateTimeKind.Utc ? now : DateTime.SpecifyKind(now.ToUniversalTime(), DateTimeKind.Utc);
    }

    private static BusinessException Denied() => new(AIErrorCodes.KnowledgeReviewDenied);
}
