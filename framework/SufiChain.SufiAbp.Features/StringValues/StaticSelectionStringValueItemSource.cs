namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Static selection string value item source.
/// </summary>
public class StaticSelectionStringValueItemSource : Volo.Abp.Validation.StringValues.StaticSelectionStringValueItemSource
{
    /// <summary>
    /// Creates a static selection item source.
    /// </summary>
    public StaticSelectionStringValueItemSource(params LocalizableSelectionStringValueItem[] items)
        : base(items)
    {
    }
}
