using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.SufiAI.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

public class MongoKnowledgeRelationProposalRepository :
    MongoDbRepository<AIMongoDbContext, KnowledgeRelationProposal, Guid>, IKnowledgeRelationProposalRepository
{
    public MongoKnowledgeRelationProposalRepository(IMongoDbContextProvider<AIMongoDbContext> provider) : base(provider) { }

    public virtual async Task<KnowledgeRelationProposal?> FindForVerificationAsync(Guid decisionId, Guid? tenantId,
        CancellationToken cancellationToken = default) =>
        await (await GetQueryableAsync()).FirstOrDefaultAsync(
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
