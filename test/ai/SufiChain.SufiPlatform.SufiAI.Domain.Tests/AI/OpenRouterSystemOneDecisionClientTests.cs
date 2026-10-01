using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class OpenRouterSystemOneDecisionClientTests
{
    [Fact]
    public async Task Successful_decision_logs_input_and_output_tokens_without_the_workspace_output_price()
    {
        using var handler = new StatusHandler(HttpStatusCode.OK, """
            {"answers":{"refund":{"type":"noul","noul":0.98}},"usage":{"input_tokens":476,"output_tokens":70,"cost":0.000019992}}
            """);
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(
            AICapabilityType.Decisions,
            "typesafe/jev-1.13",
            apiEndpoint: "https://openrouter.ai/api/v1",
            apiKey: "sk-route",
            inputPrice: 0.042m);
        AIUsageRecord? recorded = null;
        var client = CreateClient(handler, workspace, route, record => recorded = record);

        var result = await client.TryDecideAsync(Request(workspace.Id));

        result.ShouldNotBeNull();
        result.InputTokens.ShouldBe(476);
        result.OutputTokens.ShouldBe(70);
        recorded.ShouldNotBeNull();
        recorded.CapabilityType.ShouldBe(AICapabilityType.Decisions);
        recorded.IsSuccess.ShouldBeTrue();
        recorded.InputTokens.ShouldBe(476);
        recorded.OutputTokens.ShouldBe(70);
        recorded.TotalTokens.ShouldBe(546);
        recorded.Configuration.ModelConfigurationId.ShouldBe(route.Id);
        recorded.Configuration.InputPrice.ShouldBe(0.042m);
        recorded.Configuration.OutputPrice.ShouldBeNull();
        handler.LastUri!.AbsolutePath.ShouldBe("/api/v1/systemone");
    }

    [Fact]
    public void Request_body_omits_a_null_criteria_field_and_keeps_a_real_one()
    {
        var without = OpenRouterSystemOneDecisionClient.SerializeBody(
            "typesafe/jev-1.13",
            "ping",
            new Dictionary<string, SystemOneQuestion>
            {
                ["ready"] = new() { Type = "noul", Instructions = "The state is the word ping." }
            });
        without.ShouldNotContain("criteria");

        var withCriteria = OpenRouterSystemOneDecisionClient.SerializeBody(
            "typesafe/jev-1.13",
            "ping",
            new Dictionary<string, SystemOneQuestion>
            {
                ["ready"] = new()
                {
                    Type = "noul",
                    Instructions = "The state is the word ping.",
                    Criteria = new Dictionary<string, string>
                    {
                        ["true"] = "The state is exactly the word ping.",
                        ["false"] = "The state is anything else."
                    }
                }
            });
        withCriteria.ShouldContain("\"criteria\"");
        withCriteria.ShouldContain("\"true\"");
    }

    [Fact]
    public async Task Failed_decision_logs_an_unsuccessful_row()
    {
        using var handler = new StatusHandler(HttpStatusCode.BadGateway, "{}");
        var workspace = CreateWorkspace();
        var route = workspace.AddModelConfiguration(
            AICapabilityType.Decisions,
            "typesafe/jev-1.13",
            apiKey: "sk-route");
        AIUsageRecord? recorded = null;
        var client = CreateClient(handler, workspace, route, record => recorded = record);

        var result = await client.TryDecideAsync(Request(workspace.Id));

        result.ShouldBeNull();
        recorded.ShouldNotBeNull();
        recorded.IsSuccess.ShouldBeFalse();
        recorded.ErrorMessage.ShouldBe("SystemOneRequestFailed");
        recorded.CapabilityType.ShouldBe(AICapabilityType.Decisions);
        recorded.Configuration.ModelConfigurationId.ShouldBe(route.Id);
    }

    private static OpenRouterSystemOneDecisionClient CreateClient(
        StatusHandler handler,
        Workspace workspace,
        AIModelConfiguration route,
        Action<AIUsageRecord> capture)
    {
        var workspaces = Substitute.For<IWorkspaceRepository>();
        workspaces.FindAsync(workspace.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(workspace);

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient().Returns(_ => new HttpClient(handler));

        var credentials = Substitute.For<IAICredentialResolver>();
        credentials.DecryptApiKey(Arg.Any<string?>()).Returns(call => call.Arg<string?>());

        var runtime = Substitute.For<IWorkspaceRuntimeConfigurationResolver>();
        runtime.Resolve(workspace, AICapabilityType.Decisions, route, false).Returns(new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            ModelConfiguration = route,
            CapabilityType = AICapabilityType.Decisions,
            Provider = AIProviderType.OpenRouter,
            ModelId = route.ModelId,
            InputPrice = route.InputPrice,
            InputPriceUnit = route.InputPriceUnit,
            OutputPrice = 10m,
            OutputPriceUnit = AIPriceUnit.PerMillionTokens,
            ModelConfigurationId = route.Id,
            IsConfigured = true,
            IsReady = true
        });

        var usage = Substitute.For<IAIUsageRecorder>();
        usage.RecordAsync(Arg.Do<AIUsageRecord>(capture), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        return new OpenRouterSystemOneDecisionClient(
            workspaces,
            factory,
            credentials,
            runtime,
            usage,
            NullLogger<OpenRouterSystemOneDecisionClient>.Instance);
    }

    private static Workspace CreateWorkspace()
    {
        var workspace = new Workspace(Guid.NewGuid(), "decisions", AIProviderType.OpenRouter, "chat");
        workspace.UpdateConfiguration("chat", "sk-workspace", "https://openrouter.ai/api/v1");
        return workspace;
    }

    private static SystemOneDecisionRequest Request(Guid workspaceId)
    {
        return new SystemOneDecisionRequest
        {
            WorkspaceId = workspaceId,
            State = "I was charged twice.",
            Questions = new Dictionary<string, SystemOneQuestion>
            {
                ["refund"] = new() { Type = "noul", Instructions = "Is the customer asking for money back?" }
            }
        };
    }

    private sealed class StatusHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public StatusHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
