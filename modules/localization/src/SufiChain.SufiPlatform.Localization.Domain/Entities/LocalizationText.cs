using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Localization.Entities;

/// <summary>
/// Represents a localization text entry that can override or extend base translations.
/// Supports multi-tenancy to allow tenant-specific translations.
/// </summary>
public class LocalizationText : AuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// Tenant ID for multi-tenancy support. Null means host-level translation.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// The resource name this text belongs to (e.g., "FileManager", "Identity")
    /// </summary>
    public string ResourceName { get; private set; } = default!;

    /// <summary>
    /// The culture/language code (e.g., "en", "fa", "ar", "en-US")
    /// </summary>
    public string CultureName { get; private set; } = default!;

    /// <summary>
    /// The localization key (e.g., "Menu:FileManager", "Permission:Create")
    /// </summary>
    public string Key { get; private set; } = default!;

    /// <summary>
    /// The translated value
    /// </summary>
    public string Value { get; private set; } = default!;

    protected LocalizationText()
    {
    }

    public LocalizationText(
        Guid id,
        Guid? tenantId,
        string resourceName,
        string cultureName,
        string key,
        string value) : base(id)
    {
        TenantId = tenantId;
        SetResourceName(resourceName);
        SetCultureName(cultureName);
        SetKey(key);
        SetValue(value);
    }

    public void SetResourceName(string resourceName)
    {
        ResourceName = Check.NotNullOrWhiteSpace(resourceName, nameof(resourceName), maxLength: 128);
    }

    public void SetCultureName(string cultureName)
    {
        CultureName = Check.NotNullOrWhiteSpace(cultureName, nameof(cultureName), maxLength: 16);
    }

    public void SetKey(string key)
    {
        Key = Check.NotNullOrWhiteSpace(key, nameof(key), maxLength: 512);
    }

    public void SetValue(string value)
    {
        Value = Check.NotNullOrWhiteSpace(value, nameof(value), maxLength: 4096);
    }

    public void UpdateValue(string value)
    {
        SetValue(value);
    }
}
