namespace SufiChain.SufiPlatform.UI.Extensibility.TableColumns;

/// <summary>
/// A dictionary of table columns organized by entity type.
/// </summary>
public class TableColumnDictionary : Dictionary<Type, List<TableColumn>>
{
    /// <summary>
    /// Gets the columns for the specified entity type.
    /// Creates a new list if none exists.
    /// </summary>
    public List<TableColumn> Get<TEntity>()
    {
        return Get(typeof(TEntity));
    }

    /// <summary>
    /// Gets the columns for the specified entity type.
    /// Creates a new list if none exists.
    /// </summary>
    public List<TableColumn> Get(Type entityType)
    {
        if (!ContainsKey(entityType))
        {
            this[entityType] = new List<TableColumn>();
        }

        return this[entityType];
    }

    /// <summary>
    /// Adds a column for the specified entity type.
    /// </summary>
    public TableColumnDictionary AddColumn<TEntity>(TableColumn column)
    {
        Get<TEntity>().Add(column);
        return this;
    }

    /// <summary>
    /// Adds a column for the specified entity type.
    /// </summary>
    public TableColumnDictionary AddColumn(Type entityType, TableColumn column)
    {
        Get(entityType).Add(column);
        return this;
    }
}
