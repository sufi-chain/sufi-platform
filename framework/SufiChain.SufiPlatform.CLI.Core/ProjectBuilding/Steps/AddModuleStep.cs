using SufiChain.SufiPlatform.CLI.Modules;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures modules in the generated solution based on user selection.
/// 
/// This step:
/// - Removes module references that are not included
/// - Updates dependency lists in module configurations
/// - Processes module-conditional markers
/// </summary>
public class AddModuleStep : ProjectBuildPipelineStep
{
    private readonly ModuleRegistry _moduleRegistry;
    
    public AddModuleStep()
    {
        _moduleRegistry = new ModuleRegistry();
    }
    
    public override string Description => "Configuring modules...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Resolve all modules that should be included (with dependencies)
        var includedModules = _moduleRegistry.ResolveWithDependencies(context.Args.IncludedModules);
        var includedModuleKeys = new HashSet<string>(
            includedModules.Select(m => m.Key), 
            StringComparer.OrdinalIgnoreCase);
        
        // Get all modules to determine which are excluded
        var allModules = _moduleRegistry.GetAllModules();
        var excludedModules = allModules
            .Where(m => !includedModuleKeys.Contains(m.Key))
            .ToList();
        
        // Process .csproj files to remove excluded module references
        ProcessCsprojFiles(context, excludedModules);
        
        // Process module files to remove excluded modules
        ProcessModuleFiles(context, excludedModules);
        
        return Task.CompletedTask;
    }
    
    private void ProcessCsprojFiles(ProjectBuildContext context, List<ModuleDefinition> excludedModules)
    {
        var csprojFiles = context.Files.Keys.Where(f => f.EndsWith(".csproj")).ToList();
        
        foreach (var csprojFile in csprojFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[csprojFile]);
            var modified = false;
            
            foreach (var module in excludedModules)
            {
                // Skip core modules - they are never removed
                if (module.IsCore)
                    continue;
                
                // Remove PackageReference entries for this module
                var packagePattern = $@"<PackageReference\s+Include=""{Regex.Escape(module.NuGetPackagePrefix)}[^""]*""[^>]*/>\s*\n?";
                var newContent = Regex.Replace(content, packagePattern, "", RegexOptions.IgnoreCase);
                
                // Also remove ProjectReference entries for this module
                var projectPattern = $@"<ProjectReference\s+Include=""[^""]*{Regex.Escape(module.NuGetPackagePrefix)}[^""]*\.csproj""[^>]*/>\s*\n?";
                newContent = Regex.Replace(newContent, projectPattern, "", RegexOptions.IgnoreCase);
                
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
    
    private void ProcessModuleFiles(ProjectBuildContext context, List<ModuleDefinition> excludedModules)
    {
        // NOTE: Primary .cs cleanup (typeof, using, config blocks) is handled by
        // TemplateMarkerProcessorStep via conditional TEMPLATE-REMOVE IF-NOT="module:xxx" markers
        // in the host module files. This method serves as a safety net for any fully-qualified
        // typeof() references that might exist outside of marked regions.
        
        var csFiles = context.Files.Keys.Where(f => f.EndsWith(".cs")).ToList();
        
        foreach (var csFile in csFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[csFile]);
            var modified = false;
            
            foreach (var module in excludedModules)
            {
                // Skip core modules
                if (module.IsCore)
                    continue;
                
                // Remove fully-qualified typeof() references in DependsOn arrays
                // e.g. typeof(SufiChain.SufiPlatform.FileManager.Blazor.FileManagerBlazorModule),
                var typeofPattern = $@"\s*typeof\s*\(\s*{Regex.Escape(module.NuGetPackagePrefix)}[^)]*\)\s*,?\s*\n?";
                var newContent = Regex.Replace(content, typeofPattern, "", RegexOptions.IgnoreCase);
                
                // Clean up any trailing commas before closing bracket
                newContent = Regex.Replace(newContent, @",\s*\]", "]");
                newContent = Regex.Replace(newContent, @",\s*\)", ")");
                
                if (newContent != content)
                {
                    content = newContent;
                    modified = true;
                }
            }
            
            if (modified)
            {
                context.Files[csFile] = Encoding.UTF8.GetBytes(content);
            }
        }
    }
}
