using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the solution for tiered architecture.
/// </summary>
public class ConfigureTieredStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring tiered architecture...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (!context.Args.IsTiered)
            return Task.CompletedTask;

        // Tiered is the default template configuration
        // The demo solution is already configured for tiered architecture
        // with HttpApi.Host as the auth server + API and Blazor.WebApp as the frontend
        
        // Add tiered symbol for any conditional processing
        context.Symbols.Add("tiered");
        
        return Task.CompletedTask;
    }
}
