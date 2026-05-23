using System;

namespace SufiChain.SufiAbp.PermissionManagement;

public class IsGrantedRequest
{
    public Guid UserId { get; set; }

    public string[] PermissionNames { get; set; }
}
