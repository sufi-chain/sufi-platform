using System.Reflection;

namespace SufiChain.SufiPlatform.UI.Routing;

/// <summary>
/// Options for configuring the Blazor router.
/// </summary>
public class SufiRouterOptions
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
    public SufiRouterOptions()
    {
        AdditionalAssemblies = new RouterAssemblyList();
    }
}
