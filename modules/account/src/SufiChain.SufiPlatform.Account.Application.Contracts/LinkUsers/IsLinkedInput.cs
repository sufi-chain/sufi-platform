using System;

namespace SufiChain.SufiPlatform.Account;

public class IsLinkedInput
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }
}
