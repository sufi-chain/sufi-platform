using System;
using SufiChain.SufiAbp.Data;
using Volo.Abp.Data;

namespace SufiChain.SufiAbp.Users;

public interface IRoleData : IHasExtraProperties
{
    Guid Id { get; }

    Guid? TenantId { get; }

    string Name { get; }

    bool IsDefault { get;  }

    bool IsStatic { get; }

    bool IsPublic { get; }
}
