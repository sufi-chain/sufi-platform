using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Xml.Linq;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Generates Directory.Build.props with SufiVersion property for the generated solution.
/// Reads version from repository's src/versions.props during CLI execution.
/// </summary>
public class GenerateDirectoryBuildPropsStep : ProjectBuildPipelineStep
{
    public override string Description => "Generating Directory.Build.props...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var sufiVersion = GetSufiVersionFromRepo();
        
        var content = GenerateDirectoryBuildPropsContent(sufiVersion);
        
        context.Files["Directory.Build.props"] = Encoding.UTF8.GetBytes(content);
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Reads SufiVersion from repository's src/versions.props.
    /// </summary>
    private string GetSufiVersionFromRepo()
    {
        // Try to find versions.props relative to CLI assembly location
        var assemblyLocation = typeof(GenerateDirectoryBuildPropsStep).Assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var cliDir = Path.GetDirectoryName(assemblyLocation);
            if (cliDir != null)
            {
                // Navigate up to repo root: bin/Debug/net10.0 -> ../../../../../..
                var repoRoot = Path.GetFullPath(Path.Combine(cliDir, "..", "..", "..", "..", "..", ".."));
                var versionsPropsPath = Path.Combine(repoRoot, "src", "versions.props");
                
                if (File.Exists(versionsPropsPath))
                {
                    try
                    {
                        var doc = XDocument.Load(versionsPropsPath);
                        var sufiVersionElement = doc.Descendants("SufiVersion").FirstOrDefault();
                        if (sufiVersionElement != null && !string.IsNullOrWhiteSpace(sufiVersionElement.Value))
                        {
                            return sufiVersionElement.Value;
                        }
                    }
                    catch
                    {
                        // Fall through to default
                    }
                }
            }
        }
        
        // Fallback to hardcoded version
        return "0.0.0-rc.1.0";
    }
    
    /// <summary>
    /// Generates Directory.Build.props XML content.
    /// </summary>
    private string GenerateDirectoryBuildPropsContent(string sufiVersion)
    {
        return $@"<Project>
  <PropertyGroup>
    <LangVersion>latest</LangVersion>
    <SufiVersion>{sufiVersion}</SufiVersion>
  </PropertyGroup>
</Project>
";
    }
}
