using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

public class Tenant : FullAuditedAggregateRoot<Guid>, IHasEntityVersion
{
    public virtual string Name { get; protected set; }

    public virtual string NormalizedName { get; protected set; }

    public virtual Guid? EditionId { get; protected set; }

    public virtual Guid? OwnerUserId { get; protected set; }

    public virtual string? DatabaseName { get; protected set; }

    public virtual string? PrimarySubdomain { get; protected set; }

    public virtual int EntityVersion { get; protected set; }

    public virtual List<TenantConnectionString> ConnectionStrings { get; protected set; }

    public virtual List<TenantDomain> Domains { get; protected set; }

    protected Tenant()
    {
        ConnectionStrings = new List<TenantConnectionString>();
        Domains = new List<TenantDomain>();
    }

    protected internal Tenant(Guid id, [NotNull] string name, [CanBeNull] string normalizedName)
        : base(id)
    {
        SetName(name);
        SetNormalizedName(normalizedName);

        ConnectionStrings = new List<TenantConnectionString>();
        Domains = new List<TenantDomain>();
    }

    [CanBeNull]
    public virtual string FindDefaultConnectionString()
    {
        return FindConnectionString(Data.ConnectionStrings.DefaultConnectionStringName);
    }

    [CanBeNull]
    public virtual string FindConnectionString(string name)
    {
        return ConnectionStrings.FirstOrDefault(c => c.Name == name)?.Value;
    }

    public virtual void SetDefaultConnectionString(string connectionString)
    {
        SetConnectionString(Data.ConnectionStrings.DefaultConnectionStringName, connectionString);
    }

    public virtual void SetConnectionString(string name, string connectionString)
    {
        var tenantConnectionString = ConnectionStrings.FirstOrDefault(x => x.Name == name);

        if (tenantConnectionString != null)
        {
            tenantConnectionString.SetValue(connectionString);
        }
        else
        {
            ConnectionStrings.Add(new TenantConnectionString(Id, name, connectionString));
        }

        AddLocalEvent(new TenantChangedEvent(Id, NormalizedName));
    }

    public virtual void RemoveDefaultConnectionString()
    {
        RemoveConnectionString(Data.ConnectionStrings.DefaultConnectionStringName);
    }

    public virtual void RemoveConnectionString(string name)
    {
        var tenantConnectionString = ConnectionStrings.FirstOrDefault(x => x.Name == name);

        if (tenantConnectionString != null)
        {
            ConnectionStrings.Remove(tenantConnectionString);
            AddLocalEvent(new TenantChangedEvent(Id, NormalizedName));
        }
    }

    protected internal virtual void SetName([NotNull] string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), TenantConsts.MaxNameLength);
    }

    protected internal virtual void SetNormalizedName([CanBeNull] string normalizedName)
    {
        NormalizedName = normalizedName;
    }

    public virtual void SetEditionId(Guid? editionId)
    {
        EditionId = editionId;
    }

    public virtual void SetOwnerUserId(Guid? ownerUserId)
    {
        OwnerUserId = ownerUserId;
    }

    public virtual void SetDatabaseName(string databaseName)
    {
        databaseName = Check.NotNullOrWhiteSpace(
            databaseName,
            nameof(databaseName),
            TenantConsts.MaxDatabaseNameLength);

        if (databaseName.Any(character =>
                !IsAsciiLetterOrDigit(character) &&
                character != '_'))
        {
            throw new ArgumentException(
                "Tenant database name may contain only ASCII letters, digits, and underscores.",
                nameof(databaseName));
        }

        if (!DatabaseName.IsNullOrWhiteSpace() &&
            !string.Equals(DatabaseName, databaseName, StringComparison.Ordinal))
        {
            throw new BusinessException(TenantsErrorCodes.DatabaseNameIsImmutable);
        }

        DatabaseName = databaseName;
    }

    protected internal virtual void ConfigureRouting(
        string primarySubdomain,
        IEnumerable<TenantDomain> domains)
    {
        PrimarySubdomain = TenantDomainName.NormalizeSubdomain(primarySubdomain);
        Domains.Clear();
        Domains.AddRange(domains);
    }

    /// <summary>
    /// Adds a verified, active custom domain, or marks an existing custom domain verified and active.
    /// A generated domain with the same host is left unchanged.
    /// </summary>
    public virtual bool UpsertVerifiedCustomDomain(Guid id, string host)
    {
        var normalized = TenantDomainName.NormalizeHost(host);
        var existing = Domains.FirstOrDefault(domain =>
            string.Equals(domain.Host, normalized, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            if (existing.Type != TenantDomainType.Custom)
            {
                return false;
            }

            if (existing.IsVerified && existing.IsActive)
            {
                return false;
            }

            existing.SetStatus(true, true);
            return true;
        }

        Domains.Add(new TenantDomain(id, Id, normalized, TenantDomainType.Custom, true, true));
        return true;
    }

    /// <summary>Removes a custom domain. Generated domains stay.</summary>
    public virtual bool RemoveCustomDomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        string normalized;
        try
        {
            normalized = TenantDomainName.NormalizeHost(host);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var existing = Domains.FirstOrDefault(domain =>
            domain.Type == TenantDomainType.Custom &&
            string.Equals(domain.Host, normalized, StringComparison.OrdinalIgnoreCase));
        if (existing == null)
        {
            return false;
        }

        Domains.Remove(existing);
        return true;
    }

    private static bool IsAsciiLetterOrDigit(char character)
    {
        return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';
    }
}
