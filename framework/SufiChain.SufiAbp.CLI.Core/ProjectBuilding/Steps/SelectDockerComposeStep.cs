using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Selects the appropriate docker-compose template based on database provider,
/// renames it to docker-compose.yml, and deletes all other docker-compose templates.
/// </summary>
public class SelectDockerComposeStep : ProjectBuildPipelineStep
{
    public override string Description => "Selecting docker-compose template...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        var dockerDir = "etc/docker/";
        var dockerFiles = context.Files.Keys
            .Where(f => f.StartsWith(dockerDir) && f.Contains("docker-compose") && f.EndsWith(".template"))
            .ToList();

        if (!dockerFiles.Any())
        {
            // No docker-compose templates found, skip
            return Task.CompletedTask;
        }

        // Determine which template to use based on symbols
        string selectedTemplate = DetermineDockerComposeTemplate(context.Symbols);
        string selectedFilePath = $"{dockerDir}{selectedTemplate}";

        if (!context.Files.ContainsKey(selectedFilePath))
        {
            throw new InvalidOperationException(
                $"Docker-compose template '{selectedTemplate}' not found. Available templates: {string.Join(", ", dockerFiles)}");
        }

        // Rename selected template to docker-compose.yml
        var targetPath = "docker-compose.yml";
        context.Files[targetPath] = context.Files[selectedFilePath];

        // Delete all docker-compose templates (including the selected one)
        foreach (var templateFile in dockerFiles)
        {
            context.Files.Remove(templateFile);
        }

        // Also remove the base template if it exists
        var baseTemplate = $"{dockerDir}docker-compose.base.yml";
        if (context.Files.ContainsKey(baseTemplate))
        {
            context.Files.Remove(baseTemplate);
        }
        
        // Remove the entire etc/docker directory from output
        var dockerDirFiles = context.Files.Keys
            .Where(f => f.StartsWith(dockerDir))
            .ToList();
        foreach (var file in dockerDirFiles)
        {
            context.Files.Remove(file);
        }

        return Task.CompletedTask;
    }

    private string DetermineDockerComposeTemplate(HashSet<string> symbols)
    {
        // Check for MongoDB
        if (symbols.Contains("db:mongodb"))
        {
            return "docker-compose.mongodb.yml.template";
        }

        // Check for EF Core providers
        if (symbols.Contains("efp:sqlserver"))
        {
            return "docker-compose.efcore-sqlserver.yml.template";
        }

        if (symbols.Contains("efp:postgresql"))
        {
            return "docker-compose.efcore-postgresql.yml.template";
        }

        if (symbols.Contains("efp:mysql"))
        {
            return "docker-compose.efcore-mysql.yml.template";
        }

        if (symbols.Contains("efp:mariadb"))
        {
            return "docker-compose.efcore-mariadb.yml.template";
        }

        if (symbols.Contains("efp:sqlite"))
        {
            return "docker-compose.efcore-sqlite.yml.template";
        }

        // Default to SQL Server if db:efcore is set but no specific provider
        if (symbols.Contains("db:efcore"))
        {
            return "docker-compose.efcore-sqlserver.yml.template";
        }

        // Fallback to SQL Server
        return "docker-compose.efcore-sqlserver.yml.template";
    }
}
