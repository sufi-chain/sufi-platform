namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;

/// <summary>
/// Executes a sequence of project build steps.
/// </summary>
public class ProjectBuildPipeline
{
    private readonly List<ProjectBuildPipelineStep> _steps = new();
    
    /// <summary>
    /// Adds a step to the pipeline.
    /// </summary>
    public ProjectBuildPipeline AddStep(ProjectBuildPipelineStep step)
    {
        _steps.Add(step);
        return this;
    }
    
    /// <summary>
    /// Adds multiple steps to the pipeline.
    /// </summary>
    public ProjectBuildPipeline AddSteps(IEnumerable<ProjectBuildPipelineStep> steps)
    {
        _steps.AddRange(steps);
        return this;
    }
    
    /// <summary>
    /// Executes all steps in the pipeline.
    /// </summary>
    /// <param name="context">The build context containing files and configuration.</param>
    /// <param name="reportProgress">Optional callback invoked before each step with the step description.</param>
    public async Task ExecuteAsync(ProjectBuildContext context, Action<string>? reportProgress = null)
    {
        foreach (var step in _steps)
        {
            reportProgress?.Invoke(step.Description);
            await step.ExecuteAsync(context);
        }
    }
}
