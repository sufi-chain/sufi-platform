using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiPlatform.Editions;

public class Edition : FullAuditedAggregateRoot<Guid>
{
    public virtual string Name { get; protected set; } = string.Empty;

    public virtual string DisplayName { get; protected set; } = string.Empty;

    public virtual string Code { get; protected set; } = string.Empty;

    public virtual bool IsActive { get; protected set; } = true;

    protected Edition()
    {
    }

    public Edition(Guid id, string name, string displayName, string code, bool isActive = true)
        : base(id)
    {
        SetName(name);
        SetDisplayName(displayName);
        SetCode(code);
        IsActive = isActive;
    }

    public virtual void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), EditionConsts.MaxNameLength);
    }

    public virtual void SetDisplayName(string displayName)
    {
        DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), EditionConsts.MaxDisplayNameLength);
    }

    public virtual void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), EditionConsts.MaxCodeLength).Trim().ToUpperInvariant();
    }

    public virtual void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}
