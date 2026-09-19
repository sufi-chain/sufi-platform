using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

/// <summary>Read-only approval verification, not a mutation lock or human review endpoint.</summary>
[ExposeServices(typeof(ITagRelationApprovalVerifier), typeof(KnowledgeRelationApprovalVerifier))]
public class KnowledgeRelationApprovalVerifier : ITagRelationApprovalVerifier, ITransientDependency
{
    private readonly IKnowledgeRelationProposalRepository _repository;
    private readonly IKnowledgeProposalEvidenceValidator[] _evidence;
    private readonly ICurrentTenant _tenant;
    private readonly IClock _clock;

    public KnowledgeRelationApprovalVerifier(IKnowledgeRelationProposalRepository repository,
        IEnumerable<IKnowledgeProposalEvidenceValidator> evidence, ICurrentTenant tenant, IClock clock)
    {
        _repository = repository;
        _evidence = evidence.ToArray();
        _tenant = tenant;
        _clock = clock;
    }

    public virtual async Task<bool> IsApprovedAsync(TagRelationMutationRequest request, Guid? tenantId,
        Guid actorId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_tenant.Id != tenantId || actorId == Guid.Empty || _evidence.Length != 1) return false;
        var proposal = await _repository.FindForVerificationAsync(request.ApprovalDecisionId, tenantId, cancellationToken);
        if (proposal == null || !proposal.Authorizes(request, tenantId, actorId, _clock.Now) ||
            await _repository.HasLaterVersionAsync(tenantId, proposal.ProposalId, proposal.ProposalVersion, cancellationToken))
            return false;
        var evidence = new KnowledgeProposalEvidence(proposal.TenantId, proposal.ScopeType, proposal.ScopeId,
            proposal.EvidenceType, proposal.EvidenceId, proposal.EvidenceVersion, proposal.EvidenceSha256, proposal.EvidenceLocator);
        if (!await _evidence[0].IsCurrentAndAccessibleAsync(evidence, actorId, cancellationToken)) return false;
        cancellationToken.ThrowIfCancellationRequested();
        // Reload without EF tracking so evidence validation cannot hide a concurrent revoke.
        // The future delivery protocol still needs write-time ordering across databases.
        var current = await _repository.FindForVerificationAsync(request.ApprovalDecisionId, tenantId, cancellationToken);
        if (current == null || current.Version != proposal.Version ||
            await _repository.HasLaterVersionAsync(tenantId, proposal.ProposalId, proposal.ProposalVersion, cancellationToken))
            return false;
        cancellationToken.ThrowIfCancellationRequested();
        return _tenant.Id == tenantId && current.Authorizes(request, tenantId, actorId, _clock.Now);
    }
}
