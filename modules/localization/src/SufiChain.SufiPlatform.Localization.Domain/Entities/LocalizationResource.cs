using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Localization.Entities;

/// <summary>
/// Represents a localization resource configuration stored in the database.
/// This allows dynamic resource discovery for external localization stores.
/// </summary>
public class LocalizationResource : AuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// Tenant ID for multi-tenancy support. Null means host-level resource.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The unique resource name (e.g., "FileManager", "CustomModule")
    /// </summary>
    public string ResourceName { get; private set; } = default!;

    /// <summary>
    /// The default culture for this resource (e.g., "en")
    /// </summary>
    public string DefaultCulture { get; private set; } = "en";

    /// <summary>
    /// Whether this resource is enabled
    /// </summary>
    public bool IsEnabled { get; private set; } = true;

    /// <summary>
    /// Display name for the resource
    /// </summary>
    public string? DisplayName { get; private set; }

    /// <summary>
    /// Base resource names that this resource inherits from
    /// </summary>
    public List<string> BaseResourceNames { get; private set; } = new();

    protected LocalizationResource()
    {
    }

    public LocalizationResource(
        Guid id,
        Guid? tenantId,
        string resourceName,
        string defaultCulture = "en",
        string? displayName = null) : base(id)
    {
        TenantId = tenantId;
        SetResourceName(resourceName);
        SetDefaultCulture(defaultCulture);
        DisplayName = displayName;
    }

    public void SetResourceName(string resourceName)
    {
        ResourceName = Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName), maxLength: 128);
    }

    public void SetDefaultCulture(string defaultCulture)
    {
        DefaultCulture = Check.NotNullOrWhiteSpace(defaultCulture, nameof(defaultCulture), maxLength: 16);
    }

    public void SetDisplayName(string? displayName)
    {
        DisplayName = displayName;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void AddBaseResource(string resourceName)
    {
        if (!BaseResourceNames.Contains(resourceName))
        {
            BaseResourceNames.Add(resourceName);
        }
    }

    public void RemoveBaseResource(string resourceName)
    {
        BaseResourceNames.Remove(resourceName);
    }
}
