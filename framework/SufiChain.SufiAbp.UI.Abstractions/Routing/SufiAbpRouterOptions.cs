using System.Reflection;

namespace SufiChain.SufiAbp.UI.Routing;

/// <summary>
/// Options for configuring the Blazor router.
/// </summary>
public class SufiAbpRouterOptions
{
    /// <summary>
    /// The main application assembly containing routable components.
    /// </summary>
    public Assembly AppAssembly { get; set; } = default!;

    /// <summary>
    /// Additional assemblies to scan for routable components.
    /// </summary>
    public RouterAssemblyList AdditionalAssemblies { get; }

    /// <summary>
    /// Creates a new RouterOptions.
    /// </summary>
    public SufiAbpRouterOptions()
    {
        AdditionalAssemblies = new RouterAssemblyList();
    }
}
