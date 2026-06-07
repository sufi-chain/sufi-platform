using SufiChain.SufiAbp.Localization;
namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Feature definition context exposed by SufiAbp modules.
/// </summary>
public interface IFeatureDefinitionContext
{
    /// <summary>
    /// Adds a feature group.
    /// </summary>
    FeatureGroupDefinition AddGroup(string name, LocalizableString? displayName = null);

    /// <summary>
    /// Gets a feature group if it exists.
    /// </summary>
    FeatureGroupDefinition? GetGroupOrNull(string name);

    /// <summary>
    /// Removes a feature group.
    /// </summary>
    void RemoveGroup(string name);
}

/// <summary>
/// Wraps the underlying ABP feature definition context.
/// </summary>
public class FeatureDefinitionContext : IFeatureDefinitionContext
{
    private readonly Volo.Abp.Features.IFeatureDefinitionContext _context;

    /// <summary>
    /// Creates a new feature definition context wrapper.
    /// </summary>
    public FeatureDefinitionContext(Volo.Abp.Features.IFeatureDefinitionContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public virtual FeatureGroupDefinition AddGroup(string name, LocalizableString? displayName = null)
    {
        return new FeatureGroupDefinition(_context.AddGroup(name, displayName?.ToVolo()));
    }

    /// <inheritdoc />
    public virtual FeatureGroupDefinition? GetGroupOrNull(string name)
    {
        var group = _context.GetGroupOrNull(name);
        return group == null ? null : new FeatureGroupDefinition(group);
    }

    /// <inheritdoc />
    public virtual void RemoveGroup(string name)
    {
        _context.RemoveGroup(name);
    }
}

/// <summary>
/// SufiAbp feature group definition.
/// </summary>
public class FeatureGroupDefinition
{
    private readonly Volo.Abp.Features.FeatureGroupDefinition _group;

    /// <summary>
    /// Creates a new feature group wrapper.
    /// </summary>
    public FeatureGroupDefinition(Volo.Abp.Features.FeatureGroupDefinition group)
    {
        _group = group;
    }

    /// <summary>
    /// Unique group name.
    /// </summary>
    public string Name => _group.Name;

    /// <summary>
    /// Localized display name.
    /// </summary>
    public Volo.Abp.Localization.ILocalizableString DisplayName => _group.DisplayName;

    /// <summary>
    /// Features directly attached to this group.
    /// </summary>
    public IReadOnlyList<FeatureDefinition> Features => _group.Features.Select(feature => new FeatureDefinition(feature)).ToList();

    /// <summary>
    /// Adds a feature to the group.
    /// </summary>
    public virtual FeatureDefinition AddFeature(
        string name,
        string? defaultValue = null,
        LocalizableString? displayName = null,
        LocalizableString? description = null,
        IStringValueType? valueType = null,
        bool isVisibleToClients = true,
        bool isAvailableToHost = true)
    {
        return new FeatureDefinition(
            _group.AddFeature(
                name,
                defaultValue,
                displayName?.ToVolo(),
                description?.ToVolo(),
                valueType,
                isVisibleToClients,
                isAvailableToHost));
    }

    /// <summary>
    /// Sets a custom property.
    /// </summary>
    public virtual FeatureGroupDefinition WithProperty(string key, object value)
    {
        _group.WithProperty(key, value);
        return this;
    }

    /// <summary>
    /// Gets all features including child features.
    /// </summary>
    public virtual List<FeatureDefinition> GetFeaturesWithChildren()
    {
        return _group.GetFeaturesWithChildren().Select(feature => new FeatureDefinition(feature)).ToList();
    }
}

/// <summary>
/// SufiAbp feature definition.
/// </summary>
public class FeatureDefinition
{
    private readonly Volo.Abp.Features.FeatureDefinition _feature;

    /// <summary>
    /// Creates a new feature wrapper.
    /// </summary>
    public FeatureDefinition(Volo.Abp.Features.FeatureDefinition feature)
    {
        _feature = feature;
    }

    /// <summary>
    /// Unique feature name.
    /// </summary>
    public string Name => _feature.Name;

    /// <summary>
    /// Localized display name.
    /// </summary>
    public Volo.Abp.Localization.ILocalizableString DisplayName => _feature.DisplayName;

    /// <summary>
    /// Localized description.
    /// </summary>
    public Volo.Abp.Localization.ILocalizableString? Description => _feature.Description;

    /// <summary>
    /// Parent feature if this is a child feature.
    /// </summary>
    public FeatureDefinition? Parent => _feature.Parent == null ? null : new FeatureDefinition(_feature.Parent);

    /// <summary>
    /// Child features.
    /// </summary>
    public IReadOnlyList<FeatureDefinition> Children => _feature.Children.Select(feature => new FeatureDefinition(feature)).ToList();

    /// <summary>
    /// Default feature value.
    /// </summary>
    public string? DefaultValue => _feature.DefaultValue;

    /// <summary>
    /// Whether host can use this feature.
    /// </summary>
    public bool IsAvailableToHost => _feature.IsAvailableToHost;

    /// <summary>
    /// Feature value type.
    /// </summary>
    public object? ValueType => _feature.ValueType;

    /// <summary>
    /// Feature value type name.
    /// </summary>
    public string? ValueTypeName => _feature.ValueType?.Name;

    /// <summary>
    /// Feature value type runtime name.
    /// </summary>
    public string? ValueTypeRuntimeName => _feature.ValueType?.GetType().Name;

    /// <summary>
    /// Checks if a feature value is valid for this feature definition.
    /// </summary>
    public bool IsValidValue(string value)
    {
        return _feature.ValueType?.Validator.IsValid(value) != false;
    }

    /// <summary>
    /// Custom feature properties.
    /// </summary>
    public Dictionary<string, object?> Properties => _feature.Properties;

    /// <summary>
    /// Adds a child feature.
    /// </summary>
    public virtual FeatureDefinition CreateChild(
        string name,
        string? defaultValue = null,
        LocalizableString? displayName = null,
        LocalizableString? description = null,
        IStringValueType? valueType = null,
        bool isVisibleToClients = true,
        bool isAvailableToHost = true)
    {
        return new FeatureDefinition(
            _feature.CreateChild(
                name,
                defaultValue,
                displayName?.ToVolo(),
                description?.ToVolo(),
                valueType,
                isVisibleToClients,
                isAvailableToHost));
    }

    /// <summary>
    /// Sets a custom property.
    /// </summary>
    public virtual FeatureDefinition WithProperty(string key, object value)
    {
        _feature.WithProperty(key, value);
        return this;
    }
}
