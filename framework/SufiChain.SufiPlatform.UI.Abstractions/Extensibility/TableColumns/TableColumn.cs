using System.Globalization;
using SufiChain.SufiPlatform.UI.Extensibility.EntityActions;

namespace SufiChain.SufiPlatform.UI.Extensibility.TableColumns;

/// <summary>
/// Represents a column in a data table/grid.
/// </summary>
public class TableColumn
{
    /// <summary>
    /// The column header title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The data binding expression or field name.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The column width (CSS value).
    /// </summary>
    public string? Width { get; set; }

    /// <summary>
    /// The property name on the entity.
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Display format string for the value.
    /// </summary>
    public string? DisplayFormat { get; set; }

    /// <summary>
    /// Format provider for the display format.
    /// </summary>
    public IFormatProvider DisplayFormatProvider { get; set; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// Optional component type to render the cell.
    /// </summary>
    public Type? Component { get; set; }

    /// <summary>
    /// Actions available for this column (typically for action columns).
    /// </summary>
    public List<EntityAction> Actions { get; set; } = new();

    /// <summary>
    /// Custom value converter function.
    /// </summary>
    public Func<object, string>? ValueConverter { get; set; }

    /// <summary>
    /// Whether the column is sortable.
    /// </summary>
    public bool Sortable { get; set; }

    /// <summary>
    /// Whether the column is visible.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// CSS class for the column header.
    /// </summary>
    public string? HeaderClass { get; set; }

    /// <summary>
    /// CSS class for the column cells.
    /// </summary>
    public string? CellClass { get; set; }
}
