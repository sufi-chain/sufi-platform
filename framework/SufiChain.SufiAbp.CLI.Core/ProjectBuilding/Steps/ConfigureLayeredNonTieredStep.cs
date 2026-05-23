using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

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
        return Task.CompletedTask;
    }
}
