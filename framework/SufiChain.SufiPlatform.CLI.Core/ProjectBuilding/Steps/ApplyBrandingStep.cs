using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Applies branding configuration to the generated solution.
/// 
/// This step:
/// - Updates appsettings.json with App.Name and App.LogoUrl
/// - Updates branding provider classes with the app name
/// </summary>
public class ApplyBrandingStep : ProjectBuildPipelineStep
{
    public override string Description => "Applying branding configuration...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Use provided app name or default to project name
        var appName = context.Args.AppName ?? context.Args.ProjectName;
        var logoUrl = context.Args.LogoUrl;
        
        // Update appsettings.json files
        UpdateAppSettings(context, appName, logoUrl);

        // Update localized app name values
        UpdateLocalizationFiles(context, appName);
        
        // Update branding provider classes
        UpdateBrandingProviders(context, appName);
        
        return Task.CompletedTask;
    }
    
    private void UpdateAppSettings(ProjectBuildContext context, string appName, string? logoUrl)
    {
        var appSettingsFiles = context.Files.Keys
            .Where(f => f.EndsWith("appsettings.json") || f.EndsWith("appsettings.Development.json"))
            .ToList();
        
        foreach (var filePath in appSettingsFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            
            try
            {
                // Parse JSON
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Check if there's an App section to update
                if (root.TryGetProperty("App", out var appSection))
                {
                    // Rebuild the JSON with updated App section
                    var options = new JsonWriterOptions { Indented = true };
                    using var stream = new MemoryStream();
                    using var writer = new Utf8JsonWriter(stream, options);
                    
                    writer.WriteStartObject();
                    
                    foreach (var property in root.EnumerateObject())
                    {
                        if (property.Name == "App")
                        {
                            writer.WritePropertyName("App");
                            WriteAppSection(writer, appSection, appName, logoUrl);
                        }
                        else
                        {
                            property.WriteTo(writer);
                        }
                    }
                    
                    writer.WriteEndObject();
                    writer.Flush();
                    
                    context.Files[filePath] = stream.ToArray();
                }
                else
                {
                    // If no App section, try simple string replacement for common patterns
                    content = UpdateAppSettingsStringReplace(content, appName, logoUrl);
                    context.Files[filePath] = Encoding.UTF8.GetBytes(content);
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, try simple string replacement
                content = UpdateAppSettingsStringReplace(content, appName, logoUrl);
                context.Files[filePath] = Encoding.UTF8.GetBytes(content);
            }
        }
    }
    
    private void WriteAppSection(Utf8JsonWriter writer, JsonElement appSection, string appName, string? logoUrl)
    {
        writer.WriteStartObject();
        
        foreach (var property in appSection.EnumerateObject())
        {
            if (property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteString("Name", appName);
            }
            else if (property.Name.Equals("LogoUrl", StringComparison.OrdinalIgnoreCase) && logoUrl != null)
            {
                writer.WriteString("LogoUrl", logoUrl);
            }
            else
            {
                property.WriteTo(writer);
            }
        }
        
        // Add LogoUrl if it doesn't exist and was provided
        if (logoUrl != null && !appSection.TryGetProperty("LogoUrl", out _))
        {
            writer.WriteString("LogoUrl", logoUrl);
        }
        
        writer.WriteEndObject();
    }
    
    private string UpdateAppSettingsStringReplace(string content, string appName, string? logoUrl)
    {
        // Replace common placeholder patterns
        content = Regex.Replace(
            content,
            @"""Name"":\s*""[^""]*""",
            $"\"Name\": \"{appName}\"",
            RegexOptions.IgnoreCase);
        
        if (logoUrl != null)
        {
            content = Regex.Replace(
                content,
                @"""LogoUrl"":\s*""[^""]*""",
                $"\"LogoUrl\": \"{logoUrl}\"",
                RegexOptions.IgnoreCase);
        }
        
        return content;
    }

    private void UpdateLocalizationFiles(ProjectBuildContext context, string appName)
    {
        var localizationFiles = context.Files.Keys
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
                        f.Replace('\\', '/').Contains("/Localization/"))
            .ToList();

        foreach (var filePath in localizationFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            var newContent = Regex.Replace(
                content,
                @"(""AppName""\s*:\s*"")[^""]*("")",
                match => match.Groups[1].Value + appName + match.Groups[2].Value);

            if (newContent != content)
            {
                context.Files[filePath] = Encoding.UTF8.GetBytes(newContent);
            }
        }
    }
    
    private void UpdateBrandingProviders(ProjectBuildContext context, string appName)
    {
        // Find branding provider files
        var brandingFiles = context.Files.Keys
            .Where(f => f.EndsWith("BrandingProvider.cs"))
            .ToList();
        
        foreach (var filePath in brandingFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            var modified = false;
            
            // Update AppName property return value
            // public override string AppName => "DemoApp";
            var appNamePattern = @"(public\s+override\s+string\s+AppName\s*=>\s*"")[^""]*("";)";
            var newContent = Regex.Replace(content, appNamePattern, $"$1{appName}$2");
            
            if (newContent != content)
            {
                content = newContent;
                modified = true;
            }
            
            // Also handle property with getter
            // public override string AppName { get { return "DemoApp"; } }
            var appNameGetterPattern = @"(public\s+override\s+string\s+AppName\s*\{\s*get\s*\{\s*return\s*"")[^""]*(""\s*;\s*\})";
            newContent = Regex.Replace(content, appNameGetterPattern, $"$1{appName}$2");
            
            if (newContent != content)
            {
                content = newContent;
                modified = true;
            }
            
            if (modified)
            {
                context.Files[filePath] = Encoding.UTF8.GetBytes(content);
            }
        }
    }
}
