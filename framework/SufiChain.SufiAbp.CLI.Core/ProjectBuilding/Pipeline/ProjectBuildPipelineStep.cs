namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

/// <summary>
/// Base class for project build pipeline steps.
/// </summary>
public abstract class ProjectBuildPipelineStep
{
    /// <summary>
    /// Executes this pipeline step.
    /// </summary>
    public abstract Task ExecuteAsync(ProjectBuildContext context);
    
    /// <summary>
    /// Gets a description of what this step does.
    /// </summary>
    public abstract string Description { get; }
}
