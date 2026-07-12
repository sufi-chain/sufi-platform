using System;

namespace SufiChain.SufiPlatform.Identity;

[Serializable]
public class IdentityUserEmailChangedEto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string? Email { get; set; }

    public string? OldEmail { get; set; }
}
