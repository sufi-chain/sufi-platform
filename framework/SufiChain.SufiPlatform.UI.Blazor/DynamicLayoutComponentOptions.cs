namespace SufiChain.SufiPlatform.UI.Blazor;

/// <summary>
/// Options for configuring dynamic components that render in layouts.
/// </summary>
public class DynamicLayoutComponentOptions
{
    /// <summary>
    /// Dictionary of component types and their parameters to be rendered dynamically.
    /// </summary>
    public Dictionary<Type, IDictionary<string, object>?> Components { get; set; }

    /// <summary>
    /// Creates a new DynamicLayoutComponentOptions.
    /// </summary>
    public DynamicLayoutComponentOptions()
    {
        Components = new Dictionary<Type, IDictionary<string, object>?>();
    }

    /// <summary>
    /// Adds a component to be rendered dynamically.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <param name="parameters">Optional parameters for the component.</param>
    public DynamicLayoutComponentOptions Add<TComponent>(IDictionary<string, object>? parameters = null)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        Components[typeof(TComponent)] = parameters;
        return this;
    }

    /// <summary>
    /// Adds a component to be rendered dynamically.
    /// </summary>
    /// <param name="componentType">The component type.</param>
    /// <param name="parameters">Optional parameters for the component.</param>
    public DynamicLayoutComponentOptions Add(Type componentType, IDictionary<string, object>? parameters = null)
    {
        Components[componentType] = parameters;
        return this;
    }
}
