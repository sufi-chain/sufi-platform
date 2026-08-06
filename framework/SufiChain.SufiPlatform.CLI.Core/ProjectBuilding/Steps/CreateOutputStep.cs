using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Creates the output directory structure and writes files.
/// </summary>
public class CreateOutputStep : ProjectBuildPipelineStep
{
    public override string Description => "Writing output files...";

    public override async Task ExecuteAsync(ProjectBuildContext context)
    {
        var outputDir = context.Args.OutputDirectory;

        // Create output directory if it doesn't exist
        if (Directory.Exists(outputDir))
        {
            // Check if directory is empty
            if (Directory.GetFileSystemEntries(outputDir).Length > 0)
            {
                throw new InvalidOperationException(
                    $"Output directory '{outputDir}' is not empty. Please specify an empty directory or delete the existing contents.");
            }
        }
        else
        {
            Directory.CreateDirectory(outputDir);
        }

        // Write all files
        foreach (var file in context.Files)
        {
            // Skip files marked for removal
            if (context.FilesToRemove.Contains(file.Key))
                continue;

            var fullPath = Path.Combine(outputDir, file.Key);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(fullPath, file.Value);
        }
    }
}
