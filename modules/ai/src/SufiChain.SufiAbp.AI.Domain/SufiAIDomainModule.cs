using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using SufiChain.SufiAbp.AI.MCP.Execution;
using SufiChain.SufiAbp.AI.MCP.Internal;
using SufiChain.SufiAbp.AI.MCP.Registry;
using SufiChain.SufiAbp.AI.RAG;
using SufiChain.SufiAbp.AI.RAG.Services;
using SufiChain.SufiAbp.AI.Storage;

namespace SufiChain.SufiAbp.AI;

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
        
        // Register file storage service (conditional based on File-Manager availability)
        ConfigureFileStorage(context);
        
        // WorkspaceSyncService is auto-registered via ITransientDependency
        // WorkspaceAccessor is auto-registered via ITransientDependency
        
        // Note: MCP services are explicitly registered above instead of relying on
        // convention-based registration to ensure they're available when needed.
        // This prevents DI resolution issues in complex module dependency scenarios.
    }

    private void ConfigureFileStorage(ServiceConfigurationContext context)
    {
        // Check if File-Manager module is available by checking for FileItemManager
        var isFileManagerAvailable = context.Services.Any(d => 
            d.ServiceType.FullName?.Contains("SufiChain.SufiAbp.FileManager.FileItems.FileItemManager") == true);

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
