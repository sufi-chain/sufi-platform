using System;
using JetBrains.Annotations;
using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Users;

public interface IUserData : IHasExtraProperties
{
    System.Guid Id { get; }

    System.Guid? TenantId { get; }

    string UserName { get; }

    string Name { get; }

    string Surname { get; }

    bool IsActive { get; }

    [CanBeNull]
    string Email { get; }

    bool EmailConfirmed { get; }

    [CanBeNull]
    string PhoneNumber { get; }

    bool PhoneNumberConfirmed { get; }
}
