using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Renames solution, projects, namespaces, and file contents based on replacements.
/// </summary>
public class SolutionRenameStep : ProjectBuildPipelineStep
{
    public override string Description => "Renaming solution and projects...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // First pass: rename file contents
        RenameFileContents(context);
        
        // Second pass: rename file paths
        RenameFilePaths(context);
        
        return Task.CompletedTask;
    }

    private void RenameFileContents(ProjectBuildContext context)
    {
        var filesToProcess = context.Files.Keys.ToList();
        
        foreach (var filePath in filesToProcess)
        {
            if (!FileEntry.IsTextFile(filePath))
                continue;

            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            var originalContent = content;

            // Apply replacements (longer strings first to avoid partial replacements)
            var sortedReplacements = context.Replacements
                .OrderByDescending(r => r.Key.Length)
                .ToList();

            foreach (var replacement in sortedReplacements)
            {
                // Case-sensitive replacement
                content = content.Replace(replacement.Key, replacement.Value);
                
                // Also handle PascalCase versions
                var pascalKey = ToPascalCase(replacement.Key);
                var pascalValue = ToPascalCase(replacement.Value);
                if (pascalKey != replacement.Key)
                {
                    content = content.Replace(pascalKey, pascalValue);
                }
            }

            if (content != originalContent)
            {
                context.Files[filePath] = Encoding.UTF8.GetBytes(content);
            }
        }
    }

    private void RenameFilePaths(ProjectBuildContext context)
    {
        var filesToRename = context.Files.Keys
            .Where(path => context.Replacements.Keys.Any(key => path.Contains(key)))
            .ToList();

        foreach (var oldPath in filesToRename)
        {
            var newPath = oldPath;
            
            // Apply replacements (longer strings first)
            var sortedReplacements = context.Replacements
                .OrderByDescending(r => r.Key.Length)
                .ToList();

            foreach (var replacement in sortedReplacements)
            {
                newPath = newPath.Replace(replacement.Key, replacement.Value);

                var pascalKey = ToPascalCase(replacement.Key);
                var pascalValue = ToPascalCase(replacement.Value);
                if (pascalKey != replacement.Key)
                {
                    newPath = newPath.Replace(pascalKey, pascalValue);
                }
            }

            if (newPath != oldPath)
            {
                var content = context.Files[oldPath];
                context.Files.Remove(oldPath);
                context.Files[newPath] = content;
            }
        }
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Split by dots, underscores, or hyphens
        var parts = Regex.Split(input, @"[\.\-_]+");
        
        var result = new StringBuilder();
        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;
                
            result.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
                result.Append(part.Substring(1));
        }

        return result.ToString();
    }
}
