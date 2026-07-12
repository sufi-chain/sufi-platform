using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.RAG;

public class RagIndexingState : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int IndexedDocuments { get; set; }
    public DateTime? LastIndexedAt { get; set; }
    public bool IsIndexing { get; set; }
    public string? ErrorMessage { get; set; }

    protected RagIndexingState()
    {
    }

    public RagIndexingState(Guid id, Guid? tenantId, string workspaceName, string sourceName)
        : base(id)
    {
        TenantId = tenantId;
        WorkspaceName = workspaceName;
        SourceName = sourceName;
    }

    public override object[] GetKeys()
    {
        return new object[] { Id };
    }
}
