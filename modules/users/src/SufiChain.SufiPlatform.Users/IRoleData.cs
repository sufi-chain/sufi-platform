using System;
using SufiChain.SufiPlatform.Data;

using Volo.Abp.ObjectExtending;
namespace SufiChain.SufiPlatform.Users;

public interface IRoleData : IHasExtraProperties
{
    System.Guid Id { get; }

    System.Guid? TenantId { get; }

    string Name { get; }

    bool IsDefault { get;  }

    bool IsStatic { get; }

    bool IsPublic { get; }
}
