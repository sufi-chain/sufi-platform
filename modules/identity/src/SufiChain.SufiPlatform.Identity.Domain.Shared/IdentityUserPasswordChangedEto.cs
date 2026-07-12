using System;

namespace SufiChain.SufiPlatform.Identity;

[Serializable]
public class IdentityUserPasswordChangedEto
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }
}
