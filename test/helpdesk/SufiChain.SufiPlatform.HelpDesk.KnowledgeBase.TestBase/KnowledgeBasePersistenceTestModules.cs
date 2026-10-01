using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.RAG;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(HelpDeskKnowledgeBaseDomainModule))]
public class HelpDeskKnowledgeBaseTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // The domain RAG indexing handler needs a workspace resolver, which the application layer
        // supplies in a real host. Persistence tests have no AI workspace, so indexing is skipped.
        context.Services.TryAddTransient<IKBProjectAIWorkspaceResolver, UnmappedKBProjectAIWorkspaceResolver>();
    }
}

public class UnmappedKBProjectAIWorkspaceResolver : IKBProjectAIWorkspaceResolver
{
    public Task<KBResolvedAIWorkspace?> ResolveRagWorkspaceAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<KBResolvedAIWorkspace?>(null);

    public Task<string?> ResolveRagWorkspaceNameAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}
