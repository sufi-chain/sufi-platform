namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Describes a provider setting field for UI form generation.
/// </summary>
public class ProviderSettingField
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public bool IsSensitive { get; set; }
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public string? DefaultValue { get; set; }
    public string? ValidationPattern { get; set; }
}