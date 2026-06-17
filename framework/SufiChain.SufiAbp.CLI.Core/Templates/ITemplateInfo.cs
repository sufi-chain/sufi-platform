namespace SufiChain.SufiAbp.CLI.Templates;

/// <summary>
/// Defines information about a solution template.
/// </summary>
public interface ITemplateInfo
{
    /// <summary>
    /// Unique name of the template.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Display name for the template.
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Description of what the template creates.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Supported database providers.
    /// </summary>
    IReadOnlyList<string> SupportedDatabaseProviders { get; }
    
    /// <summary>
    /// Whether the template supports tiered architecture.
    /// </summary>
    bool SupportsTiered { get; }
    
    /// <summary>
    /// Whether the template supports WebApp architecture.
    /// </summary>
    bool SupportsSingle { get; }
    
    /// <summary>
    /// The base solution name used in the template (for replacement).
    /// </summary>
    string BaseSolutionName { get; }
    
    /// <summary>
    /// The base company name used in the template (for replacement).
    /// </summary>
    string BaseCompanyName { get; }
    
    /// <summary>
    /// The base project name used in the template (for replacement).
    /// </summary>
    string BaseProjectName { get; }
}
