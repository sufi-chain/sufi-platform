using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Removes host projects that are not included in the build configuration.
/// 
/// Host to project mapping:
/// - WebApp: Blazor.WebApp, Blazor.WebApp.Client
/// - WebSite: Blazor.WebSite, Blazor.WebSite.Client
/// - Web: Web
/// - HttpApi: HttpApi.Host (only removed for WebApp architecture)
/// </summary>
public class RemoveHostStep : ProjectBuildPipelineStep
{
    public override string Description => "Removing excluded host projects...";

    /// <summary>
    /// Maps host types to their associated project suffixes.
    /// </summary>
    private static readonly Dictionary<HostType, string[]> HostProjectMappings = new()
    {
        [HostType.WebApp] = new[] { "Blazor.WebApp", "Blazor.WebApp.Client" },
        [HostType.AuthServer] = new[] { "AuthServer" },
        [HostType.WebSite] = new[] { "Blazor.WebSite", "Blazor.WebSite.Client" },
        [HostType.Web] = new[] { "Web" },
        [HostType.HttpApi] = new[] { "HttpApi.Host" }
    };

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Get all host types
        var allHosts = Enum.GetValues<HostType>();
        
        foreach (var hostType in allHosts)
        {
            // Skip if this host is included
            if (context.Args.IncludedHosts.Contains(hostType))
            {
                continue;
            }
            
            // Special case: HttpApi.Host is not removed for tiered or layered
            // (layered has HttpApi.Host but no AuthServer; tiered has both)
            if (hostType == HostType.HttpApi &&
                (context.Args.IsTiered || context.Args.IncludedHosts.Contains(HostType.HttpApi)))
            {
                continue;
            }
            
            // Mark projects for removal
            if (HostProjectMappings.TryGetValue(hostType, out var projectSuffixes))
            {
                foreach (var suffix in projectSuffixes)
                {
                    var projectName = $"{context.Args.SolutionName}.{suffix}";
                    context.ProjectsToRemove.Add(projectName);
                }
            }
        }
        
        return Task.CompletedTask;
    }
}
