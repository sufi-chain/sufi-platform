using SufiChain.SufiAbp.CLI.Args;
using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the solution for MongoDB database provider.
/// </summary>
public class SwitchToMongoDbStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring for MongoDB...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        if (context.Args.DatabaseProvider != DatabaseProvider.MongoDB)
            return Task.CompletedTask;

        // MongoDB is the default template, so we mainly need to remove EF Core project if present
        var efCoreProjectName = $"{context.Args.SolutionName}.EntityFrameworkCore";
        context.ProjectsToRemove.Add(efCoreProjectName);
        
        // Update connection strings to MongoDB format
        UpdateConnectionStrings(context);
        
        return Task.CompletedTask;
    }

    private void UpdateConnectionStrings(ProjectBuildContext context)
    {
        var appSettingsFiles = context.Files.Keys
            .Where(f => f.EndsWith("appsettings.json") || f.EndsWith("appsettings.Development.json"))
            .ToList();

        foreach (var file in appSettingsFiles)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            
            // Ensure MongoDB connection string format
            var dbName = context.Args.CompanyName + context.Args.ProjectName;
            
            // Replace any SQL Server connection string with MongoDB
            if (content.Contains("Server=") || content.Contains("Data Source="))
            {
                content = System.Text.RegularExpressions.Regex.Replace(
                    content,
                    @"""Default"":\s*""[^""]+""",
                    $@"""Default"": ""mongodb://localhost:27017/{dbName}"""
                );
                context.Files[file] = Encoding.UTF8.GetBytes(content);
            }
        }
    }
}
