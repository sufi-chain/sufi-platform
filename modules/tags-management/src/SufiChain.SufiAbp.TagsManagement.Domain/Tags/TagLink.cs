using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public class TagLink : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid TagId { get; protected set; }
    public string EntityType { get; protected set; } = string.Empty;
    public Guid EntityId { get; protected set; }

    protected TagLink()
    {
    }

    public TagLink(Guid id, Guid tagId, string entityType, Guid entityId, Guid? tenantId = null) : base(id)
    {
        TagId = tagId;
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType), TagScopeConsts.MaxEntityTypeLength);
        EntityId = entityId;
        TenantId = tenantId;
    }
}

