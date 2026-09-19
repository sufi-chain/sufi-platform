using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Tags.Relations;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

[RemoteService(IsEnabled = false)]
public interface IKnowledgeRelationReviewAppService : IApplicationService
{
    Task<KnowledgeRelationProposalDto> CreateAsync(CreateKnowledgeRelationProposalDto input);
    Task<KnowledgeRelationProposalDto> ApproveAsync(ReviewKnowledgeRelationDto input);
    Task<KnowledgeRelationProposalDto> RejectAsync(ReviewKnowledgeRelationDto input);
    Task<KnowledgeRelationProposalDto> RevokeAsync(ReviewKnowledgeRelationDto input);
    Task<KnowledgeRelationProposalDto> TryApplyAsync(Guid requestId);
}

public class CreateKnowledgeRelationProposalDto
{
    public Guid? ProposalId { get; set; }
    public Guid RelationId { get; set; }
    public Guid DefinitionId { get; set; }
    public int ExpectedRelationVersion { get; set; }
    public TagRelationMutationKind Action { get; set; }
    public string ScopeType { get; set; } = null!;
    public Guid ScopeId { get; set; }
    public string SourceType { get; set; } = null!;
    public Guid SourceId { get; set; }
    public string TargetType { get; set; } = null!;
    public Guid TargetId { get; set; }
    public DateTime? ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public string Reason { get; set; } = null!;
    public string EvidenceType { get; set; } = null!;
    public Guid EvidenceId { get; set; }
    public string EvidenceVersion { get; set; } = null!;
    public string EvidenceSha256 { get; set; } = null!;
    public string EvidenceLocator { get; set; } = null!;
}

public class ReviewKnowledgeRelationDto
{
    public Guid DecisionId { get; set; }
    public int ExpectedVersion { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime? ExpiresAtUtc { get; set; }
}

public class KnowledgeRelationProposalDto
{
    public Guid DecisionId { get; set; }
    public Guid ProposalId { get; set; }
    public int ProposalVersion { get; set; }
    public int ReviewVersion { get; set; }
    public KnowledgeProposalState State { get; set; }
    public Guid RequestId { get; set; }
    public Guid? ReviewerId { get; set; }
    public KnowledgeApplicationIntentState? ApplicationState { get; set; }
    public int? AppliedRelationVersion { get; set; }
}
