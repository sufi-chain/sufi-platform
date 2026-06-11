namespace SufiChain.SufiAbp.CLI.Args;

/// <summary>
/// Arguments for project building/scaffolding.
/// </summary>
public class ProjectBuildArgs
{
    /// <summary>
    /// Full solution name (e.g., "MyCompany.MyProject")
    /// </summary>
    public required string SolutionName { get; init; }
    
    /// <summary>
    /// Company name portion (e.g., "MyCompany")
    /// </summary>
    public required string CompanyName { get; init; }
    
    /// <summary>
    /// Project name portion (e.g., "MyProject")
    /// </summary>
    public required string ProjectName { get; init; }
    
    /// <summary>
    /// Database provider to use
    /// </summary>
    public required DatabaseProvider DatabaseProvider { get; init; }
    
    /// <summary>
    /// Solution structure kind (Single or Layered).
    /// </summary>
    public required SolutionKind SolutionKind { get; init; }
    
    /// <summary>
    /// Whether to create tiered architecture (separate API + Auth hosts).
    /// Only valid when <see cref="SolutionKind"/> is <see cref="SolutionKind.Layered"/>.
    /// </summary>
    public required bool IsTiered { get; init; }
    
    /// <summary>
    /// Whether to include a dedicated AuthServer host.
    /// Implied true when <see cref="IsTiered"/> is true.
    /// </summary>
    public bool IncludeAuthServer { get; init; }
    
    /// <summary>
    /// Whether to include the optional public website host.
    /// </summary>
    public bool IncludeWebSite { get; init; }
    
    /// <summary>
    /// EF Core sub-provider. Only relevant when <see cref="DatabaseProvider"/> is
    /// <see cref="DatabaseProvider.EntityFrameworkCore"/>.
    /// </summary>
    public EfProviderKind? EfProvider { get; init; }
    
    /// <summary>
    /// Connection string for the database (optional, used for EF provider setup).
    /// </summary>
    public string? ConnectionString { get; init; }
    
    /// <summary>
    /// Whether to test the database connection after scaffolding (EF only, future).
    /// </summary>
    public bool TestDbConnection { get; init; }
    
    /// <summary>
    /// Whether to generate an initial EF migration after scaffolding (EF only, future).
    /// </summary>
    public bool GenerateInitialMigration { get; init; }
    
    /// <summary>
    /// Whether to run the DbMigrator project after scaffolding (EF only, future).
    /// </summary>
    public bool RunMigrator { get; init; }
    
    /// <summary>
    /// Whether multi-tenancy is enabled.
    /// When true, tenant-management and feature-management modules are forced.
    /// </summary>
    public bool IsMultiTenancyEnabled { get; init; }
    
    /// <summary>
    /// Output directory where the solution will be created
    /// </summary>
    public required string OutputDirectory { get; init; }
    
    /// <summary>
    /// Template source name, computed from <see cref="SolutionKind"/> and <see cref="IsTiered"/>.
    /// Examples: "blazor-webapp-single", "blazor-webapp-layered", "blazor-webapp-layered-tiered"
    /// </summary>
    public string TemplateName { get; init; } = "blazor-webapp-layered-tiered";
    
    /// <summary>
    /// Hosts to include in the generated solution.
    /// Computed from <see cref="SolutionKind"/>, <see cref="IsTiered"/>, and <see cref="IncludeWebSite"/>.
    /// </summary>
    public HashSet<HostType> IncludedHosts { get; init; } = new()
    {
        HostType.HttpApi,
        HostType.AuthServer,
        HostType.WebApp
    };
    
    /// <summary>
    /// Optional demo/sample modules to include beyond the default platform modules.
    /// Real platform modules registered from sufi-abp/modules are always included.
    /// </summary>
    public HashSet<string> IncludedModules { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Application display name for branding.
    /// </summary>
    public string? AppName { get; init; }
    
    /// <summary>
    /// Logo URL for branding.
    /// </summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>
    /// Computes the template name from <see cref="SolutionKind"/> and <see cref="IsTiered"/>.
    /// </summary>
    public static string ComputeTemplateName(SolutionKind solutionKind, bool isTiered)
    {
        return solutionKind switch
        {
            SolutionKind.Single => "blazor-webapp-single",
            SolutionKind.Layered when isTiered => "blazor-webapp-layered-tiered",
            SolutionKind.Layered => "blazor-webapp-layered",
            _ => throw new ArgumentOutOfRangeException(nameof(solutionKind))
        };
    }
    
    /// <summary>
    /// Computes the included hosts from <see cref="SolutionKind"/>, <see cref="IsTiered"/>,
    /// and <see cref="IncludeWebSite"/>.
    /// </summary>
    public static HashSet<HostType> ComputeIncludedHosts(SolutionKind solutionKind, bool isTiered, bool includeWebSite)
    {
        var hosts = new HashSet<HostType>();
        
        switch (solutionKind)
        {
            case SolutionKind.Single:
                // Single: just the App host (no host splitting)
                hosts.Add(HostType.WebApp);
                break;
                
            case SolutionKind.Layered when isTiered:
                // Layered tiered: WebApp + AuthServer + HttpApi (+ optional WebSite)
                hosts.Add(HostType.WebApp);
                hosts.Add(HostType.AuthServer);
                hosts.Add(HostType.HttpApi);
                if (includeWebSite)
                    hosts.Add(HostType.WebSite);
                break;
                
            case SolutionKind.Layered:
                // Layered: WebApp (UI + Auth) + HttpApi.Host, no AuthServer
                hosts.Add(HostType.WebApp);
                hosts.Add(HostType.HttpApi);
                break;
        }
        
        return hosts;
    }
}
