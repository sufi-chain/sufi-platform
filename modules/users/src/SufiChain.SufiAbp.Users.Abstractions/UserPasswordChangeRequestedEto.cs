using System;
using SufiChain.SufiAbp.EventBus;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.Users;

[Serializable]
[EventName("SufiChain.SufiAbp.Users.UserPasswordChangeRequested")]
public class UserPasswordChangeRequestedEto : IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }
}
