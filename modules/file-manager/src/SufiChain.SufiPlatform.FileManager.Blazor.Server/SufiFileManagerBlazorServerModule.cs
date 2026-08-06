using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Blazor.Server.Services;
using SufiChain.SufiPlatform.FileManager.Blazor.Services;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Server;

/// <summary>
/// ABP Module for File Manager Blazor Server hosting.
/// Provides server-specific implementations for authentication token retrieval.
/// </summary>
[DependsOn(
    typeof(SufiFileManagerBlazorModule)
)]
public class SufiFileManagerBlazorServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register server-side access token provider
        // This uses IHttpContextAccessor which is only available in server-side Blazor
        context.Services.AddScoped<IFileUploadAccessTokenProvider, ServerFileUploadAccessTokenProvider>();
    }
}