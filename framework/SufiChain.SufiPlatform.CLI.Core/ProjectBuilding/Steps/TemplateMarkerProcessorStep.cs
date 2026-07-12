using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Processes template markers in template files (based on ABP's FileEntryExtensions).
/// Handles both unwrapped and HTML-commented markers.
/// </summary>
public class TemplateMarkerProcessorStep : ProjectBuildPipelineStep
{
    public override string Description => "Processing template markers...";
    
    private const int MaxRecursionDepth = 20;

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var filesToProcess = context.Files.Keys.ToList();
        var filesToRemove = new List<string>();
        
        foreach (var filePath in filesToProcess)
        {
            if (!FileEntry.IsTextFile(filePath))
                continue;
                
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            
            // Step 1: Remove template code based on symbols
            content = RemoveTemplateCode(content, context.Symbols);
            
            // Step 2: Uncomment TEMPLATE-ONLY sections
            content = UncommentTemplateOnlyCode(content);
            
            // Step 2: Remove all remaining marker lines
            content = RemoveTemplateCodeMarkers(content);
            
            // Step 3: Check if file is effectively empty
            if (IsFileEmpty(content, filePath))
            {
                filesToRemove.Add(filePath);
                
                if (filePath.EndsWith(".csproj"))
                {
                    var projectName = Path.GetFileNameWithoutExtension(filePath);
                    context.ProjectsToRemove.Add(projectName);
                }
                continue;
            }
            
            context.Files[filePath] = Encoding.UTF8.GetBytes(content);
        }
        
        // Remove empty files
        foreach (var file in filesToRemove)
        {
            context.Files.Remove(file);
        }
        
        return Task.CompletedTask;
    }
    
    private bool IsFileEmpty(string content, string filePath)
    {
        var trimmed = content.Trim();
        
        if (string.IsNullOrWhiteSpace(trimmed))
            return true;
        
        if (filePath.EndsWith(".csproj"))
        {
            var withoutComments = System.Text.RegularExpressions.Regex.Replace(
                trimmed, @"<!--.*?-->", "", System.Text.RegularExpressions.RegexOptions.Singleline);
            withoutComments = withoutComments.Trim();
            
            if (string.IsNullOrWhiteSpace(withoutComments))
                return true;
        }
        
        return false;
    }
    
    private string RemoveTemplateCode(string content, HashSet<string> symbols, int recursionDepth = 0)
    {
        if ((!content.Contains("<TEMPLATE-REMOVE") && !content.Contains("<!-- <TEMPLATE-REMOVE")) || 
            recursionDepth > MaxRecursionDepth)
        {
            return content;
        }
        
        var lines = content.Split('\n');
        var newLines = new List<string>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            
            if (line.Contains("<TEMPLATE-REMOVE") || line.Contains("<!-- <TEMPLATE-REMOVE"))
            {
                var marker = ParseTemplateRemoveMarker(line);
                bool shouldRemove = ShouldRemoveSection(marker, symbols);
                
                if (!shouldRemove)
                {
                    continue;
                }
                
                int innerConditionCount = 0;
                
                while (i < lines.Length - 1)
                {
                    i++;
                    
                    if (lines[i].Contains("<TEMPLATE-REMOVE") || lines[i].Contains("<!-- <TEMPLATE-REMOVE"))
                    {
                        innerConditionCount++;
                    }
                    else if (lines[i].Contains("</TEMPLATE-REMOVE>") || lines[i].Contains("<!-- </TEMPLATE-REMOVE>"))
                    {
                        if (innerConditionCount < 1)
                        {
                            break;
                        }
                        innerConditionCount--;
                    }
                }
                
                if (i < lines.Length - 1 && 
                    (lines[i + 1].Contains("<TEMPLATE-REMOVE") || lines[i + 1].Contains("<!-- <TEMPLATE-REMOVE")))
                {
                    continue;
                }
                
                continue;
            }
            
            if (i < lines.Length)
            {
                newLines.Add(line);
            }
        }
        
        var result = string.Join('\n', newLines);
        return RemoveTemplateCode(result, symbols, recursionDepth + 1);
    }
    
    
    private string UncommentTemplateOnlyCode(string content)
    {
        if (!content.Contains("<TEMPLATE-ONLY>") && !content.Contains("<!-- <TEMPLATE-ONLY>"))
        {
            return content;
        }
        
        var lines = content.Split('\n');
        var newLines = new List<string>();
        bool inTemplateOnly = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            
            // Detect start of TEMPLATE-ONLY section
            if (trimmed.Contains("<TEMPLATE-ONLY>") || trimmed.Contains("<!-- <TEMPLATE-ONLY>"))
            {
                inTemplateOnly = true;
                continue; // Skip the marker line
            }
            
            // Detect end of TEMPLATE-ONLY section
            if (trimmed.Contains("</TEMPLATE-ONLY>") || trimmed.Contains("<!-- </TEMPLATE-ONLY>"))
            {
                inTemplateOnly = false;
                continue; // Skip the marker line
            }
            
            if (inTemplateOnly)
            {
                // Skip opening <!-- after <TEMPLATE-ONLY>
                if (trimmed == "<!--")
                {
                    continue;
                }
                
                // Skip closing --> before </TEMPLATE-ONLY>
                if (trimmed == "-->")
                {
                    continue;
                }
                
                // Keep the actual content (uncommented)
                newLines.Add(line);
            }
            else
            {
                newLines.Add(line);
            }
        }
        
        return string.Join('\n', newLines);
    }
    private string RemoveTemplateCodeMarkers(string content)
    {
        if (!content.Contains("<TEMPLATE-REMOVE") && !content.Contains("</TEMPLATE-REMOVE>") &&
            !content.Contains("<!-- <TEMPLATE-REMOVE") && !content.Contains("<!-- </TEMPLATE-REMOVE>") &&
            !content.Contains("<TEMPLATE-ONLY>") && !content.Contains("</TEMPLATE-ONLY>"))
        {
            return content;
        }
        
        var lines = content.Split('\n');
        var newLines = new List<string>();
        
        foreach (var line in lines)
        {
            if (line.Contains("<TEMPLATE-REMOVE") || line.Contains("</TEMPLATE-REMOVE>") ||
                line.Contains("<!-- <TEMPLATE-REMOVE") || line.Contains("<!-- </TEMPLATE-REMOVE>") ||
                line.Contains("<TEMPLATE-ONLY>") || line.Contains("</TEMPLATE-ONLY>") ||
                line.Contains("<!-- <TEMPLATE-ONLY>") || line.Contains("<!-- </TEMPLATE-ONLY>"))
            {
                continue;
            }
            
            newLines.Add(line);
        }
        
        return string.Join('\n', newLines);
    }
    
    private TemplateRemoveMarker ParseTemplateRemoveMarker(string markerLine)
    {
        var marker = new TemplateRemoveMarker();
        
        var trimmed = markerLine.Trim()
            .Replace("//", "").Replace("@*", "").Replace("#", "")
            .Replace("<!--", "").Replace("-->", "").Replace("*@", "")
            .Replace("<TEMPLATE-REMOVE", "").Replace(">", "")
            .Trim();
        
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return marker;
        }
        
        if (trimmed.Contains("IF-NOT=", StringComparison.OrdinalIgnoreCase))
        {
            marker.IsNegativeCondition = true;
            var condition = ExtractCondition(trimmed, "IF-NOT=");
            marker.Symbols = ParseSymbols(condition);
        }
        else if (trimmed.Contains("IF=", StringComparison.OrdinalIgnoreCase))
        {
            marker.IsNegativeCondition = false;
            var condition = ExtractCondition(trimmed, "IF=");
            marker.Symbols = ParseSymbols(condition);
        }
        
        return marker;
    }
    
    private string ExtractCondition(string text, string prefix)
    {
        var startIndex = text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
            return string.Empty;
        
        var condition = text.Substring(startIndex + prefix.Length).Trim();
        condition = condition.Trim('"', '\'', ' ', '>');
        
        return condition;
    }
    
    private List<string> ParseSymbols(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return new List<string>();
        
        return condition.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
    
    private bool ShouldRemoveSection(TemplateRemoveMarker marker, HashSet<string> symbols)
    {
        if (marker.Symbols.Count == 0)
            return true;
        
        if (marker.Symbols.Count == 1)
        {
            var symbolExists = symbols.Contains(marker.Symbols[0]);
            return marker.IsNegativeCondition ? !symbolExists : symbolExists;
        }
        
        if (marker.IsNegativeCondition)
        {
            return marker.Symbols.Any(s => !symbols.Contains(s));
        }
        else
        {
            return marker.Symbols.Any(s => symbols.Contains(s));
        }
    }
}

internal class TemplateRemoveMarker
{
    public bool IsNegativeCondition { get; set; }
    public List<string> Symbols { get; set; } = new();
}
