using System;
using JetBrains.Annotations;
using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Users;

public class RoleData : IRoleData
{
    public System.Guid Id { get; set; }

    public System.Guid? TenantId { get; set; }

    public string Name { get; set; }

    public bool IsDefault { get; set; }

    public bool IsStatic { get; set; }

    public bool IsPublic { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; }

    public RoleData()
    {
        ExtraProperties = new ExtraPropertyDictionary();
    }

    public RoleData(IRoleData roleData)
    {
        Id = roleData.Id;
        Name = roleData.Name;
        IsDefault = roleData.IsDefault;
        IsStatic = roleData.IsStatic;
        IsPublic = roleData.IsPublic;
        TenantId = roleData.TenantId;
        ExtraProperties = roleData.ExtraProperties;
    }

    public RoleData(
        System.Guid id,
        [NotNull] string name,
        bool isDefault = false,
        bool isStatic = false,
        bool isPublic = false,
        System.Guid? tenantId = null,
        ExtraPropertyDictionary extraProperties = null)
    {
        Id = id;
        Name = name;
        IsDefault = isDefault;
        IsStatic = isStatic;
        IsPublic = isPublic;
        TenantId = tenantId;
        ExtraProperties = extraProperties ?? new ExtraPropertyDictionary();
    }
}
