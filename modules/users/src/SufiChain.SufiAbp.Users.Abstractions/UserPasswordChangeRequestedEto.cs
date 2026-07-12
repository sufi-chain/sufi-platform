using System;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Users;

[Serializable]
[EventName("SufiChain.SufiAbp.Users.UserPasswordChangeRequested")]
public class UserPasswordChangeRequestedEto : IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
}
