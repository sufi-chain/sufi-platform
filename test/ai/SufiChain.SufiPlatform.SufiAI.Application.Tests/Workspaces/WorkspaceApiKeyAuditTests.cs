using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Application.Tests.MCP;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// AI-31: a workspace API key is a write-only secret. Action parameters and entity
/// property changes must not keep it, the same way MCP environment JSON is skipped.
/// </summary>
public class WorkspaceApiKeyAuditTests : SufiAITestBase<WorkspaceApiKeyAuditTestModule>
{
    [Fact]
    public async Task Audited_workspace_writes_omit_the_api_key()
    {
        var store = GetRequiredService<CapturingAuditingStore>();
        var service = GetRequiredService<IWorkspaceAppService>();
        var createSecret = "sk-audit-AI31-" + Guid.NewGuid().ToString("N");
        var updateSecret = "sk-audit-AI31-" + Guid.NewGuid().ToString("N");
        var probeSecret = "sk-audit-AI31-" + Guid.NewGuid().ToString("N");

        store.Clear();
        var created = await service.CreateAsync(new CreateWorkspaceDto
        {
            Name = "audit-workspace",
            Provider = AIProviderType.OpenAI,
            Model = "gpt-4",
            ApiKey = createSecret,
            ApiBaseUrl = "https://api.openai.com/v1"
        });

        created.HasApiKey.ShouldBeTrue();
        AssertKeyIsAbsent(store, createSecret);
        Parameters(store).ShouldContain("audit-workspace");

        store.Clear();
        var updated = await service.UpdateAsync(created.Id, new UpdateWorkspaceDto
        {
            Name = "audit-workspace",
            Provider = AIProviderType.OpenAI,
            Model = "gpt-4",
            ApiKey = updateSecret,
            ApiBaseUrl = "https://api.openai.com/v1"
        });

        updated.HasApiKey.ShouldBeTrue();
        AssertKeyIsAbsent(store, updateSecret);
        AssertKeyIsAbsent(store, createSecret);

        store.Clear();
        await service.TestConnectionAsync(new TestWorkspaceConnectionInput
        {
            WorkspaceId = created.Id,
            Model = "gpt-4",
            ApiKey = probeSecret,
            ApiBaseUrl = "https://api.openai.com/v1",
            Provider = AIProviderType.OpenAI
        });

        AssertKeyIsAbsent(store, probeSecret);
        AssertKeyIsAbsent(store, updateSecret);
        Parameters(store).ShouldContain("gpt-4");
    }

    private static void AssertKeyIsAbsent(CapturingAuditingStore store, string secret)
    {
        store.Logs.ShouldNotBeEmpty();
        Parameters(store).ShouldNotContain(secret);
        foreach (var change in store.Logs.SelectMany(log => log.EntityChanges))
        {
            foreach (var property in change.PropertyChanges)
            {
                property.PropertyName.ShouldNotBe("ApiKey");
                property.OriginalValue.ShouldNotContain(secret);
                property.NewValue.ShouldNotContain(secret);
            }
        }
    }

    private static string Parameters(CapturingAuditingStore store)
    {
        return string.Join(
            "\n",
            store.Logs.SelectMany(log => log.Actions.Select(action => action.Parameters ?? string.Empty)));
    }
}

[DependsOn(typeof(McpEnvironmentAuditTestModule))]
public class WorkspaceApiKeyAuditTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(_ => new HttpClient(new SucceedingHandler()));
        context.Services.Replace(ServiceDescriptor.Singleton(factory));
    }

    private sealed class SucceedingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
