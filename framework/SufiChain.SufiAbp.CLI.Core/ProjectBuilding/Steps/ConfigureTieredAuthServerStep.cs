using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the layered-tiered architecture with AuthServer.
/// 
/// Tiered architecture has 3+ hosts:
/// - AuthServer: Blazor Server identity host (OIDC authority, login/register pages)
/// - HttpApi.Host: API-only (JWT validation pointing to AuthServer)
/// - WebApp: Admin panel (OIDC client pointing to AuthServer)
/// - WebSite: Optional public website (OIDC client pointing to AuthServer)
/// 
/// This step ensures:
/// - AuthServer:Authority URLs point to AuthServer across all appsettings.json
/// - RemoteServices:Default:BaseUrl points to HttpApi.Host in client hosts
/// - JWT validation on HttpApi.Host uses AuthServer as authority
/// - OIDC client configs on WebApp/WebSite point to AuthServer
/// </summary>
public class ConfigureTieredAuthServerStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring tiered architecture with AuthServer...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        context.Symbols.Add("tiered");
        context.Symbols.Add("arch:tiered");

        BlazorWebAppHostCleanup.RemoveApiAndLocalAuthServerHosting(context);
        
        return Task.CompletedTask;
    }
}
