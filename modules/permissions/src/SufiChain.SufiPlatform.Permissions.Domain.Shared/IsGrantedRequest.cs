using System;

namespace SufiChain.SufiPlatform.Permissions;

public class IsGrantedRequest
{
    public Guid UserId { get; set; }

    public string[] PermissionNames { get; set; }
}
