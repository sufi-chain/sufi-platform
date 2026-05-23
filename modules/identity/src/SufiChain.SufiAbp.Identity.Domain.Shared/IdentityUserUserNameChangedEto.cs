using System;

namespace SufiChain.SufiAbp.Identity;

[Serializable]
public class IdentityUserUserNameChangedEto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string? UserName { get; set; }

    public string? OldUserName { get; set; }
}
