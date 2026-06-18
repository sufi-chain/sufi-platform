using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;
using System.Text;
using System.Text.RegularExpressions;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the layered architecture (WebApp + HttpApi.Host, no AuthServer).
///
/// Layered has:
/// - HttpApi.Host: REST API with JWT validation (Authority = Blazor.WebApp URL)
/// - Blazor.WebApp: UI + Auth (OpenIddict server, login/register), calls API over HTTP
/// - No AuthServer: authentication happens in Blazor Server
///
/// The hosts/layered/ template ships with this structure. AuthServer:Authority in
/// HttpApi.Host appsettings points to Blazor.WebApp URL. RandomizePortsStep keeps
/// these in sync when ports are randomized.
/// </summary>
public class ConfigureLayeredNonTieredStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring layered architecture (WebApp + HttpApi.Host)...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        context.Symbols.Add("layered-httpapi");

        BlazorWebAppHostCleanup.RemoveApiHosting(context);
        AlignDbMigratorConnectionStringWithLayeredHost(context);

        return Task.CompletedTask;
    }

    private static void AlignDbMigratorConnectionStringWithLayeredHost(ProjectBuildContext context)
    {
        var dbMigratorSettings = context.Files.Keys
            .Where(file => file.Contains(".DbMigrator", StringComparison.OrdinalIgnoreCase) &&
                           file.EndsWith("appsettings.json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in dbMigratorSettings)
        {
            var content = Encoding.UTF8.GetString(context.Files[file]);
            var layeredDatabaseName = $"{context.Args.ProjectName}_Layered";

            content = Regex.Replace(
                content,
                @"Database=[^;\""]+",
                $"Database={layeredDatabaseName}");

            content = Regex.Replace(
                content,
                @"mongodb://localhost:27017/[^\""]+",
                $"mongodb://localhost:27017/{layeredDatabaseName}");

            context.Files[file] = Encoding.UTF8.GetBytes(content);
        }
    }
}
