using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.Knowledge;

/// <summary>Immutable evidence binding supplied by the trusted approval owner.</summary>
public sealed class KnowledgeProposalEvidence
{
    public Guid? TenantId { get; }
    public string ScopeType { get; }
    public Guid ScopeId { get; }
    public string EntityType { get; }
    public Guid EntityId { get; }
    public string Version { get; }
    public string Sha256 { get; }
    public string Locator { get; }

    public KnowledgeProposalEvidence(Guid? tenantId, string scopeType, Guid scopeId,
        string entityType, Guid entityId, string version, string sha256, string locator)
    {
        TenantId = tenantId;
        ScopeType = scopeType ?? throw new ArgumentNullException(nameof(scopeType));
        ScopeId = scopeId;
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        EntityId = entityId;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Sha256 = sha256 ?? throw new ArgumentNullException(nameof(sha256));
        Locator = locator ?? throw new ArgumentNullException(nameof(locator));
    }
}

/// <summary>
/// Owning source integration must recheck current access, exact source version,
/// locator and content digest. No default implementation permits stale evidence.
/// </summary>
public interface IKnowledgeProposalEvidenceValidator
{
    Task<bool> IsCurrentAndAccessibleAsync(KnowledgeProposalEvidence evidence, Guid actorId,
        CancellationToken cancellationToken = default);
}
