using System;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Marks a service method as an AI-callable tool that providers can discover
/// and expose to AI models.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class SufiAbpAIToolAttribute : Attribute
{
    /// <summary>
    /// Tool name (must be unique across all tools).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Human-readable description of what the tool does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Optional category for grouping tools.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether this tool requires a permission check before execution.
    /// </summary>
    public bool RequiresPermission { get; set; }

    /// <summary>
    /// Permission name required to execute this tool.
    /// </summary>
    public string? PermissionName { get; set; }

    /// <summary>
    /// Creates the attribute.
    /// </summary>
    public SufiAbpAIToolAttribute(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}
