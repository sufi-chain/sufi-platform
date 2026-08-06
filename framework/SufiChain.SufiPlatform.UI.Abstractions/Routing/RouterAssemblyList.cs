using System.Collections.Generic;
using System.Reflection;

namespace SufiChain.SufiPlatform.UI.Routing;

/// <summary>
/// A list of assemblies to scan for routable components.
/// Automatically prevents duplicate assemblies from being added.
/// </summary>
public class RouterAssemblyList : List<Assembly>
{
    /// <summary>
    /// Adds an assembly if it hasn't been added already.
    /// </summary>
    public new void Add(Assembly assembly)
    {
        if (!Contains(assembly))
        {
            base.Add(assembly);
        }
    }

    /// <summary>
    /// Adds multiple assemblies, skipping any that have already been added.
    /// </summary>
    public new void AddRange(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Add(assembly);
        }
    }
}
