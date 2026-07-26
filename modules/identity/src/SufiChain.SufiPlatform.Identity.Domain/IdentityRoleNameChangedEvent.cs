using System;

namespace SufiChain.SufiPlatform.Identity;

[Obsolete("Use the distributed event (IdentityRoleNameChangedEto) instead.")]
public class IdentityRoleNameChangedEvent
{
    public IdentityRole IdentityRole { get; set; } = default!;

    public string OldName { get; set; } = default!;
}
