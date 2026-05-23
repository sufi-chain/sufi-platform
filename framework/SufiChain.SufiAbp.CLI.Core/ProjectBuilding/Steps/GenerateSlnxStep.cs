using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Xml.Linq;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Generates a modern .slnx (XML-based solution file) instead of legacy .sln format.
/// Benefits:
/// - No GUIDs required (Visual Studio generates them automatically)
/// - Clean XML structure
/// - Easy to read and maintain
/// - Minimal format for .NET 10+
/// </summary>
public class GenerateSlnxStep : ProjectBuildPipelineStep
{
    public override string Description => "Generating solution file (.slnx)...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Remove old .sln file if exists
        var oldSlnFile = context.Files.Keys.FirstOrDefault(f => f.EndsWith(".sln"));
        if (oldSlnFile != null)
        {
            context.Files.Remove(oldSlnFile);
        }

        // Collect all .csproj files
        var projectFiles = context.Files.Keys
            .Where(f => f.EndsWith(".csproj"))
            .OrderBy(f => f)
            .ToList();

        // Group projects by folder structure
        var projectsByFolder = GroupProjectsByFolder(projectFiles);

        // Generate .slnx XML
        var slnxContent = GenerateSlnxXml(projectsByFolder);

        // Add .slnx file to context
        var slnxFileName = $"{context.Args.SolutionName}.slnx";
        var slnxPath = slnxFileName; // Root of solution
        
        // Use ASCII encoding (matches ABP's .slnx format)
        context.Files[slnxPath] = Encoding.ASCII.GetBytes(slnxContent);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Groups projects by their top-level folder (src, test, etc.)
    /// </summary>
    private Dictionary<string, List<string>> GroupProjectsByFolder(List<string> projectFiles)
    {
        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var projectFile in projectFiles)
        {
            // Normalize path separators
            var normalizedPath = projectFile.Replace('\\', '/');
            
            // Extract top-level folder (src, test, etc.)
            var parts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                var topFolder = parts[0];
                
                if (!groups.ContainsKey(topFolder))
                {
                    groups[topFolder] = new List<string>();
                }
                
                groups[topFolder].Add(projectFile);
            }
        }

        return groups;
    }

    /// <summary>
    /// Generates the .slnx XML content
    /// </summary>
    private string GenerateSlnxXml(Dictionary<string, List<string>> projectsByFolder)
    {
        var solution = new XElement("Solution");

        // Add folders and their projects
        foreach (var folder in projectsByFolder.OrderBy(kvp => kvp.Key))
        {
            var folderElement = new XElement("Folder",
                new XAttribute("Name", $"/{folder.Key}/"));

            foreach (var projectPath in folder.Value.OrderBy(p => p))
            {
                // Normalize path to forward slashes for cross-platform compatibility
                var normalizedPath = projectPath.Replace('\\', '/');
                
                folderElement.Add(new XElement("Project",
                    new XAttribute("Path", normalizedPath)));
            }

            solution.Add(folderElement);
        }

        // Return formatted XML without declaration (matches ABP format)
        var settings = new System.Xml.XmlWriterSettings
        {
            Encoding = Encoding.ASCII,
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = true, // No XML declaration for .slnx files
            NewLineChars = "\r\n"
        };
        
        var sb = new StringBuilder();
        using (var writer = System.Xml.XmlWriter.Create(sb, settings))
        {
            solution.WriteTo(writer);
        }

        return sb.ToString();
    }
}
