namespace SufiChain.SufiPlatform.CLI.Args;

/// <summary>
/// Database provider options for project generation.
/// </summary>
public enum DatabaseProvider
{
    /// <summary>
    /// Entity Framework Core with SQL Server
    /// </summary>
    EntityFrameworkCore,
    
    /// <summary>
    /// MongoDB
    /// </summary>
    MongoDB
}
