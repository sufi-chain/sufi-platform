using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using SufiChain.SufiPlatform.SufiAI.MCP.Registry;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.RAG.Services;
using SufiChain.SufiPlatform.SufiAI.Storage;
namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAIDomainSharedModule),
    typeof(SufiAIModule)
)]
public class SufiAIDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register RAG service
        context.Services.AddTransient<IRAGService, RAGService>();
        
        // Register MCP services (explicit registration to ensure they're available)
        context.Services.AddSingleton<IInternalToolDiscoveryService, ReflectionToolDiscoveryService>();
        context.Services.AddSingleton<IMCPToolRegistry, MCPToolRegistry>();
        context.Services.AddTransient<IMCPToolExecutor, MCPToolExecutionManager>();
        context.Services.AddTransient<IMCPCatalogCache, MCPCatalogCache>();
        
        // Register file storage service (conditional based on File-Manager availability)
        ConfigureFileStorage(context);
        
        // WorkspaceSyncService is auto-registered via ITransientDependency
        // WorkspaceAccessor is auto-registered via ITransientDependency
        
        // Note: MCP services are explicitly registered above instead of relying on
        // convention-based registration to ensure they're available when needed.
        // This prevents DI resolution issues in complex module dependency scenarios.
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        // The DbMigrator runs module initialization before the database exists, so warming
        // the catalog here would query a non-existent DB. Skip it in the migration
        // environment; the catalog rebuilds lazily on first runtime read.
        if (context.ServiceProvider.IsDataMigrationEnvironment())
        {
            return;
        }

        var logger = context.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<SufiAIDomainModule>();

        try
        {
            logger.LogInformation("Warming MCP catalog cache at startup");
           var cache = context.ServiceProvider.GetRequiredService<IMCPCatalogCache>();
           await cache.RebuildAsync(CancellationToken.None);
           logger.LogInformation("MCP catalog cache warmed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to warm MCP catalog cache at startup; it will rebuild on first read");
        }
    }

    private void ConfigureFileStorage(ServiceConfigurationContext context)
    {
        // Check if File-Manager module is available by checking for FileItemManager
        var isFileManagerAvailable = context.Services.Any(d => 
            d.ServiceType.FullName?.Contains("SufiChain.SufiPlatform.FileManager.FileItems.FileItemManager") == true);

        if (isFileManagerAvailable)
        {
            // Use File-Manager integration
            context.Services.Replace(ServiceDescriptor.Transient<IAIFileStorageService, FileManagerStorageService>());
            
            // Log that File-Manager integration is enabled
            // Note: Logger not available here, will log in application initialization
        }
        else
        {
            // Fallback to blob storage
            context.Services.Replace(ServiceDescriptor.Transient<IAIFileStorageService, DefaultBlobStorageService>());
            
            // Log that fallback mode is enabled
            // Note: Logger not available here, will log in application initialization
        }
    }
}
