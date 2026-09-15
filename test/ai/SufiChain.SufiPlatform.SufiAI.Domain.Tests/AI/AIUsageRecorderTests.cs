using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class AIUsageRecorderTests
{
    [Fact]
    public void Route_price_wins_over_workspace_price()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "cost-tests",
            AIProviderType.OpenAI,
            "workspace-default");
        workspace.UpdateConfiguration(
            "workspace-default",
            "sk-workspace",
            "https://api.example/v1",
            inputCostPer1MTokens: 1m,
            outputCostPer1MTokens: 2m);
        var route = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "priced-chat",
            apiEndpoint: "https://api.example/v1",
            apiKey: "sk-route",
            inputCostPer1MTokens: 10m,
            outputCostPer1MTokens: 20m);

        var configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            ModelConfiguration = route,
            CapabilityType = AICapabilityType.ChatCompletion,
            Provider = AIProviderType.OpenAI,
            ModelId = route.ModelId,
            InputCostPer1MTokens = route.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens,
            OutputCostPer1MTokens = route.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens,
            ModelConfigurationId = route.Id,
            IsConfigured = true,
            IsReady = true
        };

        var recorder = new TestableAIUsageRecorder();
        var cost = recorder.Calculate(configuration, 1000, 500, null);

        cost.IsCostCalculated.ShouldBeTrue();
        cost.EstimatedCost.ShouldBe(0.02m);
        cost.CostCalculationNote.ShouldBeNull();
    }

    [Fact]
    public async Task RecordAsync_writes_route_id_and_route_rate()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "cost-tests",
            AIProviderType.OpenAI,
            "workspace-default");
        workspace.UpdateConfiguration(
            "workspace-default",
            "sk-workspace",
            "https://api.example/v1",
            inputCostPer1MTokens: 1m,
            outputCostPer1MTokens: 2m);
        var first = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "shared-chat",
            apiEndpoint: "https://east.example/v1",
            apiKey: "sk-east",
            inputCostPer1MTokens: 10m,
            outputCostPer1MTokens: 20m);
        var second = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "shared-chat",
            apiEndpoint: "https://west.example/v1",
            apiKey: "sk-west",
            inputCostPer1MTokens: 10m,
            outputCostPer1MTokens: 20m);

        var inserted = new List<AIUsageLog>();
        var repository = Substitute.For<IAIUsageLogRepository>();
        repository.InsertAsync(Arg.Any<AIUsageLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var log = call.Arg<AIUsageLog>();
                inserted.Add(log);
                return log;
            });

        var recorder = new TestableAIUsageRecorder(repository);
        await recorder.RecordAsync(CreateRecord(workspace, first));
        await recorder.RecordAsync(CreateRecord(workspace, second));

        inserted.Count.ShouldBe(2);
        inserted.ShouldAllBe(log => log.ModelId == "shared-chat");
        inserted.Select(log => log.ModelConfigurationId).OrderBy(id => id).ShouldBe(
            new Guid?[] { first.Id, second.Id }.OrderBy(id => id));
        inserted[0].EstimatedCost.ShouldBe(0.0002m);
        inserted[0].IsCostCalculated.ShouldBeTrue();
    }

    private static AIUsageRecord CreateRecord(Workspace workspace, AIModelConfiguration route)
    {
        return new AIUsageRecord
        {
            Configuration = new WorkspaceRuntimeConfiguration
            {
                Workspace = workspace,
                ModelConfiguration = route,
                CapabilityType = AICapabilityType.ChatCompletion,
                Provider = AIProviderType.OpenAI,
                ModelId = route.ModelId,
                InputCostPer1MTokens = route.InputCostPer1MTokens ?? workspace.InputCostPer1MTokens,
                OutputCostPer1MTokens = route.OutputCostPer1MTokens ?? workspace.OutputCostPer1MTokens,
                ModelConfigurationId = route.Id,
                IsConfigured = true,
                IsReady = true
            },
            InputTokens = 10,
            OutputTokens = 5,
            LatencyMs = 8,
            IsSuccess = true
        };
    }

    private sealed class TestableAIUsageRecorder : AIUsageRecorder
    {
        public TestableAIUsageRecorder()
            : this(Substitute.For<IAIUsageLogRepository>())
        {
        }

        public TestableAIUsageRecorder(IAIUsageLogRepository repository)
            : base(repository, NullLogger<AIUsageRecorder>.Instance)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService(Arg.Any<IGuidGenerator>())
                .Returns(callInfo => callInfo.ArgAt<IGuidGenerator>(0));
            LazyServiceProvider = lazy;
        }

        public (decimal EstimatedCost, bool IsCostCalculated, string? CostCalculationNote) Calculate(
            WorkspaceRuntimeConfiguration configuration,
            int? inputTokens,
            int? outputTokens,
            int? totalTokens)
        {
            var result = CalculateCost(configuration, inputTokens, outputTokens, totalTokens);
            return (result.EstimatedCost, result.IsCostCalculated, result.CostCalculationNote);
        }
    }
}
