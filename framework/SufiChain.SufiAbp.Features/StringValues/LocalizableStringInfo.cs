namespace SufiChain.SufiAbp.Features;

/// <summary>
/// Localizable string info for selection items.
/// </summary>
public class LocalizableStringInfo : Volo.Abp.Validation.StringValues.LocalizableStringInfo
{
    /// <summary>
    /// Creates localizable string info.
    /// </summary>
    public LocalizableStringInfo(string resourceName, string name)
        : base(resourceName, name)
    {
    }
}
