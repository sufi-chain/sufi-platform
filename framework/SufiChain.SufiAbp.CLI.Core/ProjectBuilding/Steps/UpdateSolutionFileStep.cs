using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Updates the solution file to:
/// 1. Remove external project references (paths with ..\..\..)
/// 2. Remove external solution folders (modules, framework)
/// 3. Clean up nested project associations
/// 4. Regenerate GUIDs for uniqueness
/// </summary>
public class UpdateSolutionFileStep : ProjectBuildPipelineStep
{
    public override string Description => "Updating solution file...";
    
    // Known solution folder GUIDs to keep (src folder)
    private const string SrcFolderGuid = "58E47500-2571-4B38-84FE-3455689053E9";
    
    // Solution folder project type GUID
    private const string SolutionFolderTypeGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Find the solution file
        var slnFile = context.Files.Keys.FirstOrDefault(f => f.EndsWith(".sln"));
        if (slnFile == null)
            return Task.CompletedTask;

        var content = Encoding.UTF8.GetString(context.Files[slnFile]);
        
        // Step 1: Remove external project references and collect their GUIDs
        var (cleanedContent, removedProjectGuids) = RemoveExternalProjects(content);
        
        // Step 2: Remove orphaned solution folders (those with no remaining child projects)
        cleanedContent = RemoveOrphanedSolutionFolders(cleanedContent, removedProjectGuids);
        
        // Step 3: Clean up nested project associations for removed projects
        cleanedContent = CleanupNestedProjects(cleanedContent, removedProjectGuids);
        
        // Step 4: Clean up project configurations for removed projects
        cleanedContent = CleanupProjectConfigurations(cleanedContent, removedProjectGuids);
        
        // Step 5: Regenerate GUIDs for uniqueness
        cleanedContent = RegenerateProjectGuids(cleanedContent);
        
        // Step 6: Clean up multiple blank lines
        cleanedContent = Regex.Replace(cleanedContent, @"(\r?\n){3,}", "\n\n");
        
        context.Files[slnFile] = Encoding.UTF8.GetBytes(cleanedContent);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes external project references (those with relative paths pointing outside the solution).
    /// </summary>
    private (string Content, HashSet<string> RemovedGuids) RemoveExternalProjects(string content)
    {
        var removedGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = content.Split('\n').ToList();
        var result = new List<string>();
        
        var i = 0;
        while (i < lines.Count)
        {
            var line = lines[i];
            
            // Check if this is a project declaration with external path
            if (line.TrimStart().StartsWith("Project("))
            {
                // Extract path from project line: Project("{GUID}") = "Name", "path\to\project.csproj", "{ProjectGUID}"
                var pathMatch = Regex.Match(line, @"Project\([^)]+\)\s*=\s*""[^""]+"",\s*""([^""]+)""");
                if (pathMatch.Success)
                {
                    var projectPath = pathMatch.Groups[1].Value;
                    
                    // Check if this is an external path (starts with ..\..\.. or contains framework/modules)
                    if (IsExternalProject(projectPath))
                    {
                        // Extract project GUID
                        var guidMatch = Regex.Match(line, @",\s*""\{([^}]+)\}""");
                        if (guidMatch.Success)
                        {
                            removedGuids.Add(guidMatch.Groups[1].Value);
                        }
                        
                        // Skip this project declaration and its EndProject line
                        while (i < lines.Count && !lines[i].TrimStart().StartsWith("EndProject"))
                        {
                            i++;
                        }
                        i++; // Skip EndProject line
                        continue;
                    }
                }
            }
            
            result.Add(line);
            i++;
        }
        
        return (string.Join("\n", result), removedGuids);
    }
    
    /// <summary>
    /// Determines if a project path is external (outside the generated solution).
    /// </summary>
    private bool IsExternalProject(string path)
    {
        // External paths start with relative parent references pointing outside
        if (path.StartsWith(@"..\..\..\") || path.StartsWith("../../../"))
            return true;
        
        // External module/framework paths
        if (path.Contains(@"\framework\") || path.Contains("/framework/") ||
            path.Contains(@"\modules\") || path.Contains("/modules/"))
            return true;
            
        return false;
    }
    
    /// <summary>
    /// Removes solution folders that have no remaining child projects.
    /// </summary>
    private string RemoveOrphanedSolutionFolders(string content, HashSet<string> removedProjectGuids)
    {
        // Solution folders to remove (external module folders and Solution Items)
        var foldersToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "file-manager",
            "sufi-blazor", 
            "audit-logging",
            "background-jobs",
            "feature-management",
            "setting-management",
            "identity",
            "tenant-management",
            "sufi-theme",
            "framework",
            "modules",           // Parent folder for all modules
            "Solution Items"     // VS Solution Items folder (files exist but folder not needed in .sln)
        };
        
        var lines = content.Split('\n').ToList();
        var result = new List<string>();
        var folderGuidsToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        var i = 0;
        while (i < lines.Count)
        {
            var line = lines[i];
            
            // Check if this is a solution folder we want to remove
            if (line.TrimStart().StartsWith($"Project(\"{{{SolutionFolderTypeGuid}}}\""))
            {
                // Extract folder name and GUID
                var match = Regex.Match(line, @"Project\([^)]+\)\s*=\s*""([^""]+)"",\s*""[^""]+"",\s*""\{([^}]+)\}""");
                if (match.Success)
                {
                    var folderName = match.Groups[1].Value;
                    var folderGuid = match.Groups[2].Value;
                    
                    if (foldersToRemove.Contains(folderName))
                    {
                        folderGuidsToRemove.Add(folderGuid);
                        removedProjectGuids.Add(folderGuid);
                        
                        // Skip this folder declaration and its EndProject line
                        while (i < lines.Count && !lines[i].TrimStart().StartsWith("EndProject"))
                        {
                            i++;
                        }
                        i++; // Skip EndProject line
                        continue;
                    }
                }
            }
            
            result.Add(line);
            i++;
        }
        
        return string.Join("\n", result);
    }
    
    /// <summary>
    /// Removes nested project associations for removed projects.
    /// </summary>
    private string CleanupNestedProjects(string content, HashSet<string> removedGuids)
    {
        var lines = content.Split('\n').ToList();
        var result = new List<string>();
        
        foreach (var line in lines)
        {
            // Check if this line references a removed project GUID
            var shouldRemove = false;
            foreach (var guid in removedGuids)
            {
                if (line.Contains(guid, StringComparison.OrdinalIgnoreCase))
                {
                    shouldRemove = true;
                    break;
                }
            }
            
            if (!shouldRemove)
            {
                result.Add(line);
            }
        }
        
        return string.Join("\n", result);
    }
    
    /// <summary>
    /// Removes project configurations for removed projects.
    /// </summary>
    private string CleanupProjectConfigurations(string content, HashSet<string> removedGuids)
    {
        var lines = content.Split('\n').ToList();
        var result = new List<string>();
        
        foreach (var line in lines)
        {
            // Configuration lines look like: {GUID}.Debug|Any CPU.ActiveCfg = ...
            var configMatch = Regex.Match(line.Trim(), @"^\{([^}]+)\}\.");
            if (configMatch.Success)
            {
                var guid = configMatch.Groups[1].Value;
                if (removedGuids.Contains(guid))
                {
                    continue; // Skip this configuration line
                }
            }
            
            result.Add(line);
        }
        
        return string.Join("\n", result);
    }

    private string RegenerateProjectGuids(string content)
    {
        // Match GUIDs in project declarations: "{GUID}"
        var guidPattern = @"\{([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12})\}";
        var guidsToReplace = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Well-known GUIDs to preserve
        var preserveGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            SrcFolderGuid,
            SolutionFolderTypeGuid,
            "9A19103F-16F7-4668-BE54-9A1E7A4F7556", // C# SDK project type
            "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"  // C# classic project type
        };
        
        // Find all unique GUIDs (except preserved ones and solution GUID)
        foreach (Match match in Regex.Matches(content, guidPattern))
        {
            var guid = match.Groups[1].Value;
            if (!preserveGuids.Contains(guid) && 
                !guidsToReplace.ContainsKey(guid) &&
                !content.Contains($"SolutionGuid = {{{guid}}}")) // Don't replace solution GUID
            {
                guidsToReplace[guid] = Guid.NewGuid().ToString("D").ToUpperInvariant();
            }
        }
        
        // Replace all GUIDs
        foreach (var kvp in guidsToReplace)
        {
            content = content.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
        }
        
        return content;
    }
}
