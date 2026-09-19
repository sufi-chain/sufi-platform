using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

public interface IKnowledgeRelationProposalRepository : IRepository<KnowledgeRelationProposal, Guid>
{
    Task<KnowledgeRelationProposal?> FindForVerificationAsync(Guid decisionId, Guid? tenantId,
        CancellationToken cancellationToken = default);
    Task<KnowledgeRelationProposal?> FindLatestAsync(Guid proposalId, Guid? tenantId,
        CancellationToken cancellationToken = default);
    Task<bool> HasLaterVersionAsync(Guid? tenantId, Guid proposalId, int proposalVersion,
        CancellationToken cancellationToken = default);
}
