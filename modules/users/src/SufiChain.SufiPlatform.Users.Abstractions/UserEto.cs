using System;
using Volo.Abp.Data;
using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Users;

[EventName("SufiChain.SufiPlatform.Users.User")]
public class UserEto : IUserData, IMultiTenant
{
    public System.Guid Id { get; set; }

    public System.Guid? TenantId { get; set; }

    public string UserName { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public bool IsActive { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();
}
