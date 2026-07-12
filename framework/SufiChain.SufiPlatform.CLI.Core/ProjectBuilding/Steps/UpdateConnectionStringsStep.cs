using SufiChain.SufiPlatform.CLI.Args;
using SufiChain.SufiPlatform.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.Json;

namespace SufiChain.SufiPlatform.CLI.ProjectBuilding.Steps;

/// <summary>
/// Updates connection strings in appsettings files based on selected database provider.
/// Transforms SQL Server connection strings to provider-specific formats.
/// </summary>
public class UpdateConnectionStringsStep : ProjectBuildPipelineStep
{
    public override string Description => "Updating connection strings...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // Only process if EF Core is selected
        if (context.Args.DatabaseProvider != DatabaseProvider.EntityFrameworkCore)
        {
            return Task.CompletedTask;
        }
        
        // If SQL Server (default), no transformation needed
        if (!context.Args.EfProvider.HasValue || context.Args.EfProvider.Value == EfProviderKind.SqlServer)
        {
            return Task.CompletedTask;
        }
        
        var provider = context.Args.EfProvider.Value;
        var databaseName = context.Args.ProjectName; // Use project name as database name
        
        // Process all appsettings.json files
        var filesToProcess = context.Files.Keys
            .Where(f => f.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase) ||
                       f.EndsWith("appsettings.Development.json", StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        foreach (var filePath in filesToProcess)
        {
            var content = Encoding.UTF8.GetString(context.Files[filePath]);
            
            try
            {
                var jsonDoc = JsonDocument.Parse(content);
                
                // Check if ConnectionStrings section exists
                if (jsonDoc.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings))
                {
                    var newContent = TransformConnectionStrings(content, connectionStrings, provider, databaseName);
                    if (newContent != content)
                    {
                        context.Files[filePath] = Encoding.UTF8.GetBytes(newContent);
                    }
                }
                
                jsonDoc.Dispose();
            }
            catch
            {
                // If JSON parsing fails, skip this file
                continue;
            }
        }
        
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Transforms connection strings in JSON content.
    /// </summary>
    private string TransformConnectionStrings(string jsonContent, JsonElement connectionStrings, EfProviderKind provider, string databaseName)
    {
        var result = jsonContent;
        
        // Process each connection string
        foreach (var property in connectionStrings.EnumerateObject())
        {
            var oldConnectionString = property.Value.GetString();
            if (string.IsNullOrWhiteSpace(oldConnectionString))
                continue;
            
            // Only transform if it looks like a SQL Server connection string
            if (!oldConnectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
                continue;
            
            var newConnectionString = GetConnectionString(provider, databaseName);
            result = result.Replace(oldConnectionString, newConnectionString);
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets provider-specific connection string format.
    /// </summary>
    private string GetConnectionString(EfProviderKind provider, string databaseName)
    {
        return provider switch
        {
            EfProviderKind.SqlServer => 
                $"Server=localhost,1433;Database={databaseName};User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Encrypt=False",
            
            EfProviderKind.PostgreSQL => 
                $"Host=localhost;Port=5432;Database={databaseName};Username=postgres;Password=postgres",
            
            EfProviderKind.MySQL => 
                $"Server=localhost;Port=3306;Database={databaseName};Uid=root;Pwd=root",
            
            EfProviderKind.MariaDB => 
                $"Server=localhost;Port=3306;Database={databaseName};Uid=root;Pwd=root",
            
            EfProviderKind.Sqlite => 
                $"Data Source={databaseName}.db",
            
            _ => throw new ArgumentException($"Unsupported EF Core provider: {provider}")
        };
    }
}
