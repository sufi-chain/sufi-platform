using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.Identity;

public class UserData : ExtensibleObject
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public bool IsActive { get; set; }

    public string? Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public UserData()
        : base(false)
    {
    }

    public UserData(
        Guid id,
        string userName,
        string? email = null,
        string? name = null,
        string? surname = null,
        bool emailConfirmed = false,
        string? phoneNumber = null,
        bool phoneNumberConfirmed = false,
        Guid? tenantId = null,
        bool isActive = true,
        ExtraPropertyDictionary? extraProperties = null)
        : base(false)
    {
        Id = id;
        UserName = userName;
        Email = email;
        Name = name;
        Surname = surname;
        IsActive = isActive;
        EmailConfirmed = emailConfirmed;
        PhoneNumber = phoneNumber;
        PhoneNumberConfirmed = phoneNumberConfirmed;
        TenantId = tenantId;

        if (extraProperties != null)
        {
            ExtraProperties = extraProperties;
        }
    }
}
