using System;

namespace SufiChain.SufiAbp.Identity;

[Serializable]
public class IdentityRoleNameChangedEto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string Name { get; set; } = null!;

    public string? OldName { get; set; }
}
