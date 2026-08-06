using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Cleans the template by removing files not needed in scaffolded solutions.
/// </summary>
public class CleanTemplateStep : ProjectBuildPipelineStep
{
    public override string Description => "Cleaning template...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Clean up files that shouldn't be in the template
        var filesToRemove = context.Files.Keys
            .Where(f => ShouldRemoveFile(f))
            .ToList();

        foreach (var file in filesToRemove)
        {
            context.Files.Remove(file);
        }

        return Task.CompletedTask;
    }

    private bool ShouldRemoveFile(string filePath)
    {
        // Remove demo-specific files
        if (filePath.EndsWith("run-all.ps1"))
            return true;

        // Remove root README (we generate our own)
        if (filePath == "README.md" || filePath.EndsWith("\\README.md") || filePath.EndsWith("/README.md"))
        {
            // Only remove if it's at root level
            var parts = filePath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return true;
        }

        // Remove tye.yaml (demo-specific)
        if (filePath.EndsWith("tye.yaml"))
            return true;

        return false;
    }
}
