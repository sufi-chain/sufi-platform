namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Accessor helpers for selection string value items.
/// </summary>
public static class SelectionStringValueItemAccessor
{
    /// <summary>
    /// Gets selection items from a selection value type.
    /// </summary>
    public static IReadOnlyCollection<object> GetItems(object? valueType)
    {
        if (valueType is Volo.Abp.Validation.StringValues.SelectionStringValueType selectionStringValueType &&
            selectionStringValueType.ItemSource?.Items != null)
        {
            return selectionStringValueType.ItemSource.Items.Cast<object>().ToList();
        }

        return Array.Empty<object>();
    }

    /// <summary>
    /// Gets the item value.
    /// </summary>
    public static string GetValue(object item)
    {
        return ((Volo.Abp.Validation.StringValues.ISelectionStringValueItem)item).Value;
    }

    /// <summary>
    /// Gets the localizable display text resource name.
    /// </summary>
    public static string GetDisplayTextResourceName(object item)
    {
        return ((Volo.Abp.Validation.StringValues.ISelectionStringValueItem)item).DisplayText.ResourceName;
    }

    /// <summary>
    /// Gets the localizable display text name.
    /// </summary>
    public static string GetDisplayTextName(object item)
    {
        return ((Volo.Abp.Validation.StringValues.ISelectionStringValueItem)item).DisplayText.Name;
    }
}
