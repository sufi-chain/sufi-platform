namespace SufiChain.SufiAbp.UI.Extensibility.EntityActions;

/// <summary>
/// A dictionary of entity actions organized by entity type.
/// </summary>
public class EntityActionDictionary : Dictionary<Type, List<EntityAction>>
{
    /// <summary>
    /// Gets the actions for the specified entity type.
    /// Creates a new list if none exists.
    /// </summary>
    public List<EntityAction> Get<TEntity>()
    {
        return Get(typeof(TEntity));
    }

    /// <summary>
    /// Gets the actions for the specified entity type.
    /// Creates a new list if none exists.
    /// </summary>
    public List<EntityAction> Get(Type entityType)
    {
        if (!ContainsKey(entityType))
        {
            this[entityType] = new List<EntityAction>();
        }

        return this[entityType];
    }

    /// <summary>
    /// Adds an action for the specified entity type.
    /// </summary>
    public EntityActionDictionary AddAction<TEntity>(EntityAction action)
    {
        Get<TEntity>().Add(action);
        return this;
    }

    /// <summary>
    /// Adds an action for the specified entity type.
    /// </summary>
    public EntityActionDictionary AddAction(Type entityType, EntityAction action)
    {
        Get(entityType).Add(action);
        return this;
    }
}
