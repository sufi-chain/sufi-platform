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
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
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
            inputPrice: 10m,
            outputPrice: 20m);

        var configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            ModelConfiguration = route,
            CapabilityType = AICapabilityType.ChatCompletion,
            Provider = AIProviderType.OpenAI,
            ModelId = route.ModelId,
            InputPrice = route.InputPrice ?? workspace.InputCostPer1MTokens,
            InputPriceUnit = route.InputPriceUnit,
            OutputPrice = route.OutputPrice ?? workspace.OutputCostPer1MTokens,
            OutputPriceUnit = route.OutputPriceUnit,
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
    public void Minute_price_bills_audio_seconds_and_ignores_the_workspace_token_rate()
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
            inputCostPer1MTokens: 100m,
            outputCostPer1MTokens: 100m);
        var configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = workspace,
            CapabilityType = AICapabilityType.AudioTranscription,
            Provider = AIProviderType.OpenAI,
            ModelId = "openai/whisper-1",
            InputPrice = 0.006m,
            InputPriceUnit = AIPriceUnit.PerMinute,
            IsConfigured = true,
            IsReady = true
        };

        var recorder = new TestableAIUsageRecorder();
        var cost = recorder.Calculate(configuration, 1000, 500, 1500, audioSeconds: 30m);
        cost.IsCostCalculated.ShouldBeTrue();
        cost.EstimatedCost.ShouldBe(0.003m);

        var missing = recorder.Calculate(configuration, 1000, 0, 1000);
        missing.IsCostCalculated.ShouldBeFalse();
        missing.CostCalculationNote.ShouldBe(AIUsageRecorder.UsageUnavailable);
    }

    [Fact]
    public void Character_price_bills_input_text_length()
    {
        var configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = new Workspace(Guid.NewGuid(), "cost-tests", AIProviderType.OpenAI, "workspace-default"),
            CapabilityType = AICapabilityType.TextToSpeech,
            Provider = AIProviderType.OpenAI,
            ModelId = "deepgram/aura-2",
            InputPrice = 15m,
            InputPriceUnit = AIPriceUnit.PerMillionCharacters,
            IsConfigured = true,
            IsReady = true
        };

        var recorder = new TestableAIUsageRecorder();
        var cost = recorder.Calculate(configuration, null, null, null, characterCount: 1_000_000);
        cost.IsCostCalculated.ShouldBeTrue();
        cost.EstimatedCost.ShouldBe(15m);
    }

    [Fact]
    public void Decision_input_price_ignores_an_empty_output_side()
    {
        var configuration = new WorkspaceRuntimeConfiguration
        {
            Workspace = new Workspace(Guid.NewGuid(), "cost-tests", AIProviderType.OpenRouter, "workspace-default"),
            CapabilityType = AICapabilityType.Decisions,
            Provider = AIProviderType.OpenRouter,
            ModelId = "typesafe/jev-1.13",
            InputPrice = 0.042m,
            InputPriceUnit = AIPriceUnit.PerMillionTokens,
            OutputPrice = null,
            OutputPriceUnit = AIPriceUnit.PerMillionTokens,
            IsConfigured = true,
            IsReady = true
        };

        var recorder = new TestableAIUsageRecorder();
        var cost = recorder.Calculate(configuration, 476, 70, 546);
        cost.IsCostCalculated.ShouldBeTrue();
        cost.EstimatedCost.ShouldBe(0.000019992m);
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
            inputPrice: 10m,
            outputPrice: 20m);
        var second = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "shared-chat",
            apiEndpoint: "https://west.example/v1",
            apiKey: "sk-west",
            inputPrice: 10m,
            outputPrice: 20m);

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

    [Fact]
    public async Task RecordAsync_commits_under_the_caller_tenant_when_the_request_is_cancelled()
    {
        var tenantId = Guid.NewGuid();
        var workspace = new Workspace(
            Guid.NewGuid(),
            "inherited-host",
            AIProviderType.OpenAI,
            "workspace-default");
        var inserted = new List<AIUsageLog>();
        CancellationToken? insertToken = null;
        var repository = Substitute.For<IAIUsageLogRepository>();
        repository.InsertAsync(Arg.Any<AIUsageLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                insertToken = call.Arg<CancellationToken>();
                var log = call.Arg<AIUsageLog>();
                inserted.Add(log);
                return log;
            });

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager
            .Begin(Arg.Any<AbpUnitOfWorkOptions>(), true)
            .Returns(unitOfWork);

        var recorder = new TestableAIUsageRecorder(repository, unitOfWorkManager, currentTenant);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await recorder.RecordAsync(CreateRecord(workspace, workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "chat",
            inputPrice: 1m,
            outputPrice: 1m)), cancelled.Token);

        inserted.Count.ShouldBe(1);
        inserted[0].TenantId.ShouldBe(tenantId);
        insertToken.ShouldBe(CancellationToken.None);
        await unitOfWork.Received(1).CompleteAsync(CancellationToken.None);
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
                InputPrice = route.InputPrice ?? workspace.InputCostPer1MTokens,
                InputPriceUnit = route.InputPriceUnit,
                OutputPrice = route.OutputPrice ?? workspace.OutputCostPer1MTokens,
                OutputPriceUnit = route.OutputPriceUnit,
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

        public TestableAIUsageRecorder(
            IAIUsageLogRepository repository,
            IUnitOfWorkManager? unitOfWorkManager = null,
            ICurrentTenant? currentTenant = null)
            : base(repository, NullLogger<AIUsageRecorder>.Instance, unitOfWorkManager, currentTenant)
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
            int? totalTokens,
            decimal? audioSeconds = null,
            int? characterCount = null)
        {
            var result = CalculateCost(
                configuration,
                inputTokens,
                outputTokens,
                totalTokens,
                audioSeconds,
                characterCount);
            return (result.EstimatedCost, result.IsCostCalculated, result.CostCalculationNote);
        }
    }
}
