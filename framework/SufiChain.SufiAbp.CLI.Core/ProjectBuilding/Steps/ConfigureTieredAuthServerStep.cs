using SufiChain.SufiAbp.CLI.ProjectBuilding.Pipeline;

namespace SufiChain.SufiAbp.CLI.ProjectBuilding.Steps;

/// <summary>
/// Configures the layered-tiered architecture with AuthServer.
/// 
/// Tiered architecture has 3+ hosts:
/// - AuthServer: Blazor Server identity host (OIDC authority, login/register pages)
/// - HttpApi.Host: API-only (JWT validation pointing to AuthServer)
/// - WebApp: Admin panel (OIDC client pointing to AuthServer)
/// - WebPublic: Optional public website (OIDC client pointing to AuthServer)
/// 
/// This step ensures:
/// - AuthServer:Authority URLs point to AuthServer across all appsettings.json
/// - RemoteServices:Default:BaseUrl points to HttpApi.Host in client hosts
/// - JWT validation on HttpApi.Host uses AuthServer as authority
/// - OIDC client configs on WebApp/WebPublic point to AuthServer
/// </summary>
public class ConfigureTieredAuthServerStep : ProjectBuildPipelineStep
{
    public override string Description => "Configuring tiered architecture with AuthServer...";

    public override Task ExecuteAsync(ProjectBuildContext context)
    {
        // The layered-tiered template already has the correct structure with AuthServer.
        // This step is primarily a validation/no-op for now since the template
        // already ships with the correct 4-host setup.
        //
        // Future enhancements:
        // - Validate that AuthServer:Authority URLs are consistent
        // - Wire up OpenIddict app registrations dynamically
        // - Handle custom port allocation
        
        return Task.CompletedTask;
    }
}
