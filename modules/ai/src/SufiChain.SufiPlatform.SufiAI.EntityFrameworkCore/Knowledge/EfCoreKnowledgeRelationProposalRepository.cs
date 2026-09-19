using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

public class EfCoreKnowledgeRelationProposalRepository :
    EfCoreRepository<IAIDbContext, KnowledgeRelationProposal, Guid>, IKnowledgeRelationProposalRepository
{
    public EfCoreKnowledgeRelationProposalRepository(IDbContextProvider<IAIDbContext> provider) : base(provider) { }

    public virtual async Task<KnowledgeRelationProposal?> FindForVerificationAsync(Guid decisionId, Guid? tenantId,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == decisionId && x.TenantId == tenantId, cancellationToken);

    public virtual async Task<KnowledgeRelationProposal?> FindLatestAsync(Guid proposalId, Guid? tenantId,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).Where(x => x.TenantId == tenantId && x.ProposalId == proposalId)
            .OrderByDescending(x => x.ProposalVersion).FirstOrDefaultAsync(cancellationToken);

    public virtual async Task<bool> HasLaterVersionAsync(Guid? tenantId, Guid proposalId, int proposalVersion,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).AnyAsync(x => x.TenantId == tenantId &&
            x.ProposalId == proposalId && x.ProposalVersion > proposalVersion, cancellationToken);
}
