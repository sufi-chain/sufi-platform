using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.FileManager.Blazor.Services;
using SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly.Services;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.Blazor.WebAssembly;

/// <summary>
/// ABP Module for File Manager Blazor WebAssembly hosting.
/// Provides WebAssembly-specific implementations for authentication token retrieval.
/// </summary>
[DependsOn(
    typeof(SufiFileManagerBlazorModule)
)]
public class SufiFileManagerBlazorWebAssemblyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register WebAssembly access token provider
        // This uses IAccessTokenProvider which is the WebAssembly-specific way to get tokens
        context.Services.AddScoped<IFileUploadAccessTokenProvider, WebAssemblyFileUploadAccessTokenProvider>();
    }
}