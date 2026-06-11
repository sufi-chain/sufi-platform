using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using SufiChain.SufiAbp.CLI.Templates;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Reads template files from the Sufi template source.
/// Priority: explicit environment override, debug filesystem templates, release CDN zip cache.
/// </summary>
public class ReadTemplateFilesStep : ProjectBuildPipelineStep
{
    private readonly string? _templatePath;
    private readonly TemplateManager _templateManager;

    public ReadTemplateFilesStep(string? templatePath, TemplateManager templateManager)
    {
        _templatePath = templatePath;
        _templateManager = templateManager;
    }

    public override string Description => "Loading template files...";

    public override async Task ExecuteAsync(ProjectBuildContext context)
    {
        var templateName = context.Args.TemplateName ?? "blazor-webapp-layered-tiered";
        
        try
        {
            var files = await _templateManager.LoadTemplateAsync(
                templateName,
                cancellationToken: default);
            
            foreach (var kvp in files)
            {
                context.Files[kvp.Key] = kvp.Value;
            }
            
            if (context.Files.Count == 0)
            {
                throw new InvalidOperationException($"Template '{templateName}' loaded but contains no files.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            await FallbackLoadAsync(context, templateName);
        }
    }
    
    /// <summary>
    /// Fallback loading from the pipeline-provided path if the primary source fails.
    /// </summary>
    private async Task FallbackLoadAsync(ProjectBuildContext context, string templateName)
    {
        // Try filesystem path first (legacy dev mode)
        if (!string.IsNullOrEmpty(_templatePath))
        {
            await LoadFromFileSystemAsync(context, _templatePath);
            return;
        }
        
        var assemblyLocation = typeof(TemplateManager).Assembly.Location;
        var baseDir = AppContext.BaseDirectory;
        var currentDir = Directory.GetCurrentDirectory();
        
        throw new InvalidOperationException(
            $"No template files found for '{templateName}'.\n\n" +
            "Debug info:\n" +
            $"  Assembly location: {assemblyLocation}\n" +
            $"  AppContext.BaseDirectory: {baseDir}\n" +
            $"  Current directory: {currentDir}\n\n" +
            "The CLI looks for templates in this order:\n" +
            "  1. SOPHI_TEMPLATE_ZIP environment variable\n" +
            "  2. SOPHI_TEMPLATE_PATH environment variable\n" +
            "  3. Debug: sufi-abp/templates/app/aspnet-core\n" +
            "  4. Release: https://cdn.sabp.ir/templates/latest.json and versioned template zip\n\n" +
            "Solutions:\n" +
            "  1. Run from the sufi-abp repository root in Debug mode\n" +
            "  2. Set SOPHI_TEMPLATE_PATH to D:\\Projects\\SCIS\\sufi-chain\\sufi-abp\\templates\n" +
            "  3. Publish app-blazor-webapp-unified.zip under cdn.sabp.ir/templates for Release mode");
    }

    private async Task LoadFromFileSystemAsync(ProjectBuildContext context, string basePath)
    {
        var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
        
        foreach (var file in files)
        {
            // Skip common non-template files
            if (ShouldSkipFile(file))
                continue;

            var relativePath = Path.GetRelativePath(basePath, file);
            var content = await File.ReadAllBytesAsync(file);
            
            context.Files[relativePath] = content;
        }
    }

    private static bool ShouldSkipFile(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath) ?? "";

        // Skip build outputs
        if (directory.Contains("\\bin\\") || directory.Contains("/bin/") ||
            directory.Contains("\\obj\\") || directory.Contains("/obj/"))
            return true;

        // Skip user-specific files
        if (fileName.EndsWith(".user"))
            return true;

        // Skip git folder
        if (directory.Contains("\\.git") || directory.Contains("/.git"))
            return true;

        // Skip IDE folders
        if (directory.Contains("\\.vs") || directory.Contains("/.vs") ||
            directory.Contains("\\.idea") || directory.Contains("/.idea"))
            return true;

        // Skip log files and folders
        if (directory.Contains("\\Logs\\") || directory.Contains("/Logs/") ||
            directory.EndsWith("\\Logs") || directory.EndsWith("/Logs"))
            return true;

        if (fileName.EndsWith(".log") || fileName == "logs.txt")
            return true;

        // Skip package-lock files
        if (fileName == "package-lock.json")
            return true;

        // Skip node_modules
        if (directory.Contains("\\node_modules") || directory.Contains("/node_modules"))
            return true;

        return false;
    }
}
