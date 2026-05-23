using System;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.EventBus;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.Users;

[EventName("SufiChain.SufiAbp.Users.User")]
public class UserEto : IUserData, IMultiTenant
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string UserName { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public bool IsActive { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; }
}
