using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Removes specified projects from the solution and file system.
/// </summary>
public class RemoveProjectStep : ProjectBuildPipelineStep
{
    public override string Description => "Removing unused projects...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (context.ProjectsToRemove.Count == 0)
            return Task.CompletedTask;

        // Remove project files
        RemoveProjectFiles(context);
        
        // Update solution file
        UpdateSolutionFile(context);
        
        // Update project references
        UpdateProjectReferences(context);
        
        return Task.CompletedTask;
    }

    private void RemoveProjectFiles(ProjectBuildContext context)
    {
        var filesToRemove = new List<string>();

        foreach (var projectName in context.ProjectsToRemove)
        {
            // Find all files in the project folder
            var projectFolder = projectName.Replace('.', Path.DirectorySeparatorChar);
            
            foreach (var filePath in context.Files.Keys)
            {
                // Check if file is in the project folder or is the project folder
                var normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar);
                
                if (normalizedPath.Contains(projectName + Path.DirectorySeparatorChar) ||
                    normalizedPath.Contains(Path.DirectorySeparatorChar + projectName + Path.DirectorySeparatorChar) ||
                    normalizedPath.StartsWith(projectName + Path.DirectorySeparatorChar) ||
                    normalizedPath.EndsWith(Path.DirectorySeparatorChar + projectName + ".csproj") ||
                    normalizedPath.EndsWith(projectName + ".csproj"))
                {
                    filesToRemove.Add(filePath);
                }
            }
        }

        foreach (var file in filesToRemove)
        {
            context.Files.Remove(file);
            context.FilesToRemove.Add(file);
        }
    }

    private void UpdateSolutionFile(ProjectBuildContext context)
    {
        var slnFiles = context.Files.Keys.Where(f => f.EndsWith(".sln")).ToList();

        foreach (var slnFile in slnFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[slnFile]);
            var lines = content.Split('\n').ToList();
            var resultLines = new List<string>();
            var skipUntilEndProject = false;
            var projectGuidsToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // First pass: find project GUIDs to remove
            foreach (var line in lines)
            {
                foreach (var projectName in context.ProjectsToRemove)
                {
                    // Match: Project("{...}") = "ProjectName", "path\ProjectName.csproj", "{GUID}"
                    var pattern = $@"Project\(""[^""]*""\)\s*=\s*""[^""]*{Regex.Escape(projectName)}[^""]*""[^{{]*\{{([^}}]+)\}}";
                    var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        projectGuidsToRemove.Add(match.Groups[1].Value);
                    }
                }
            }

            // Second pass: remove project entries and references
            foreach (var line in lines)
            {
                var shouldSkip = false;

                // Check if this is a project line for a project to remove
                foreach (var projectName in context.ProjectsToRemove)
                {
                    if (line.Contains($"\"{projectName}\"") || 
                        line.Contains($"\\{projectName}.csproj") ||
                        line.Contains($"/{projectName}.csproj"))
                    {
                        skipUntilEndProject = true;
                        shouldSkip = true;
                        break;
                    }
                }

                // Check if line contains a GUID we're removing
                foreach (var guid in projectGuidsToRemove)
                {
                    if (line.Contains(guid, StringComparison.OrdinalIgnoreCase))
                    {
                        shouldSkip = true;
                        break;
                    }
                }

                if (skipUntilEndProject)
                {
                    if (line.Trim().StartsWith("EndProject"))
                    {
                        skipUntilEndProject = false;
                    }
                    continue;
                }

                if (!shouldSkip)
                {
                    resultLines.Add(line);
                }
            }

            context.Files[slnFile] = Encoding.UTF8.GetBytes(string.Join('\n', resultLines));
        }
    }

    private void UpdateProjectReferences(ProjectBuildContext context)
    {
        var csprojFiles = context.Files.Keys.Where(f => f.EndsWith(".csproj")).ToList();

        foreach (var csprojFile in csprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[csprojFile]);
            var modified = false;

            foreach (var projectName in context.ProjectsToRemove)
            {
                // Remove ProjectReference entries
                var pattern = $@"<ProjectReference[^>]*{Regex.Escape(projectName)}\.csproj[^>]*/>\s*\n?";
                var newContent = Regex.Replace(content, pattern, "", RegexOptions.IgnoreCase);
                
                // Also handle multi-line ProjectReference
                pattern = $@"<ProjectReference[^>]*{Regex.Escape(projectName)}\.csproj[^>]*>.*?</ProjectReference>\s*\n?";
                newContent = Regex.Replace(newContent, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (newContent != content)
                {
                    content = newContent;
                    modified = true;
                }
            }

            if (modified)
            {
                context.Files[csprojFile] = Encoding.UTF8.GetBytes(content);
            }
        }
    }
}
