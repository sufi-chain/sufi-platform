using System;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Users;

[Serializable]
[EventName("SufiChain.SufiPlatform.Users.UserPasswordChangeRequested")]
public class UserPasswordChangeRequestedEto : IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
}
