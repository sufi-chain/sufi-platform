using System;

namespace SufiChain.SufiAbp.Users;

[Serializable]
public class InviteUserToTenantRequestedEto
{
    public Guid UserId { get; set; }

    public Guid TenantId { get; set; }

    public string TenantName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string? UserName { get; set; }
}
