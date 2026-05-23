using System;

namespace SufiChain.SufiAbp.AIManagement.MCP.Attributes;

/// <summary>
/// Marks an ApplicationService method as an MCP tool that can be called by AI models.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class MCPToolAttribute : Attribute
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
    /// Whether this tool requires special permissions.
    /// </summary>
    public bool RequiresPermission { get; set; }
    
    /// <summary>
    /// Permission name required to execute this tool.
    /// </summary>
    public string? PermissionName { get; set; }

    public MCPToolAttribute(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}
