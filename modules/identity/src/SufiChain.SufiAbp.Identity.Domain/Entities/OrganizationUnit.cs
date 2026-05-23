using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Identity;

public class OrganizationUnit : FullAuditedAggregateRoot<Guid>, IMultiTenant, IHasEntityVersion
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid? ParentId { get; internal set; }

    public virtual string Code { get; internal set; } = null!;

    public virtual string DisplayName { get; set; } = null!;

    public virtual int EntityVersion { get; set; }

    public virtual ICollection<OrganizationUnitRole> Roles { get; protected set; }

    public OrganizationUnit()
    {
        Roles = new Collection<OrganizationUnitRole>();
    }

    public OrganizationUnit(Guid id, string displayName, Guid? parentId = null, Guid? tenantId = null)
        : base(id)
    {
        TenantId = tenantId;
        DisplayName = displayName;
        ParentId = parentId;
        Roles = new Collection<OrganizationUnitRole>();
    }

    public static string CreateCode(params int[] numbers)
    {
        if (numbers == null || numbers.Length == 0)
        {
            return null!;
        }

        return string.Join(".", numbers.Select(number => number.ToString("D5")));
    }

    public static string AppendCode(string? parentCode, string childCode)
    {
        Check.NotNullOrWhiteSpace(childCode, nameof(childCode));

        if (string.IsNullOrWhiteSpace(parentCode))
        {
            return childCode;
        }

        return parentCode + "." + childCode;
    }

    public static string? GetParentCode(string code)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));

        var splittedCode = code.Split('.');
        if (splittedCode.Length == 1)
        {
            return null;
        }

        return string.Join(".", splittedCode.Take(splittedCode.Length - 1));
    }

    public virtual void AddRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        if (IsInRole(roleId))
        {
            return;
        }

        Roles.Add(new OrganizationUnitRole(roleId, Id, TenantId));
    }

    public virtual void RemoveRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        Roles.RemoveAll(r => r.RoleId == roleId);
    }

    public virtual bool IsInRole(Guid roleId)
    {
        Check.NotNull(roleId, nameof(roleId));

        return Roles.Any(r => r.RoleId == roleId);
    }

    /// <summary>
    /// Gets relative code to the parent.
    /// Example: if code = "00019.00055.00001" and parentCode = "00019", returns "00055.00001".
    /// </summary>
    public static string? GetRelativeCode(string code, string parentCode)
    {
        if (code.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
        }

        if (parentCode.IsNullOrEmpty())
        {
            return code;
        }

        if (code.Length == parentCode.Length)
        {
            return null;
        }

        return code.Substring(parentCode.Length + 1);
    }

    /// <summary>
    /// Calculates next code for given code.
    /// Example: if code = "00019.00055.00001" returns "00019.00055.00002".
    /// </summary>
    public static string CalculateNextCode(string code)
    {
        if (code.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
        }

        var parentCode = GetParentCode(code);
        var lastUnitCode = GetLastUnitCode(code);

        return AppendCode(parentCode, CreateCode(Convert.ToInt32(lastUnitCode) + 1));
    }

    /// <summary>
    /// Gets the last unit code.
    /// Example: if code = "00019.00055.00001" returns "00001".
    /// </summary>
    public static string GetLastUnitCode(string code)
    {
        if (code.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
        }

        var splittedCode = code.Split('.');
        return splittedCode[splittedCode.Length - 1];
    }
}
