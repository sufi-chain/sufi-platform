using System;

namespace SufiChain.SufiPlatform.Account;

public class UnLinkUserInput
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }
}
