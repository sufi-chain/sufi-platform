using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Menus.Menus;

public class Menu : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string ContextType { get; protected set; } = string.Empty;
    public Guid? ContextId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string DisplayName { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; }

    protected Menu() { }

    public Menu(Guid id, string contextType, Guid? contextId, string name, string displayName, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ContextType = Check.NotNullOrWhiteSpace(contextType, nameof(contextType), MenusConsts.MaxContextTypeLength);
        ContextId = contextId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), MenusConsts.MaxMenuNameLength);
        SetDisplayName(displayName);
        IsActive = true;
    }

    public virtual void SetDisplayName(string displayName) => DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), MenusConsts.MaxDisplayNameLength);

    public virtual void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > MenusConsts.MaxDescriptionLength)
        {
            throw new BusinessException(MenusErrorCodes.MenuItemInvalidTarget).WithData("MaxLength", MenusConsts.MaxDescriptionLength);
        }
        Description = description;
    }

    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
