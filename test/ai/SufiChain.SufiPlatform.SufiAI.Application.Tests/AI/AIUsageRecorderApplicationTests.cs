using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

public class AIUsageRecorderApplicationTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeResolver;

    public AIUsageRecorderApplicationTests()
    {
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _runtimeResolver = GetRequiredService<IWorkspaceRuntimeConfigurationResolver>();
    }

    [Fact]
    public async Task Should_Bill_At_Route_Rate_Not_Workspace_Rate()
    {
        var seeded = await SeedWorkspaceAsync();
        var route = seeded.ModelConfigurations.Single(item => item.ModelId == "priced-chat");
        var configuration = _runtimeResolver.Resolve(
            seeded,
            AICapabilityType.ChatCompletion,
            route,
            isExplicitSelection: true);

        configuration.InputCostPer1MTokens.ShouldBe(10m);
        configuration.OutputCostPer1MTokens.ShouldBe(20m);

        await WithUnitOfWorkAsync(async services =>
        {
            await services.GetRequiredService<IAIUsageRecorder>().RecordAsync(new AIUsageRecord
            {
                Configuration = configuration,
                InputTokens = 1000,
                OutputTokens = 500,
                LatencyMs = 12,
                IsSuccess = true
            });
        });

        var logs = await WithUnitOfWorkAsync(services =>
            services.GetRequiredService<IAIUsageLogRepository>().GetByWorkspaceAsync(seeded.Id));

        logs.Count.ShouldBe(1);
        logs[0].ModelConfigurationId.ShouldBe(route.Id);
        logs[0].IsCostCalculated.ShouldBeTrue();
        logs[0].EstimatedCost.ShouldBe(0.02m);
    }

    [Fact]
    public async Task Should_Keep_Distinct_Route_Ids_When_ModelIds_Match()
    {
        var seeded = await SeedWorkspaceAsync();
        var first = seeded.ModelConfigurations.Single(item => item.ApiEndpoint == "https://east.example/v1");
        var second = seeded.ModelConfigurations.Single(item => item.ApiEndpoint == "https://west.example/v1");

        var logs = await RecordAndLoadAsync(seeded, first, second);

        logs.Count.ShouldBe(2);
        logs.ShouldAllBe(log => log.ModelId == "shared-chat");
        logs.Select(log => log.ModelConfigurationId).OrderBy(id => id).ShouldBe(
            new Guid?[] { first.Id, second.Id }.OrderBy(id => id));
    }

    [Fact]
    public async Task Mid_session_switch_attributes_each_turn_on_usage_log()
    {
        var seeded = await SeedWorkspaceAsync();
        var first = seeded.ModelConfigurations.Single(item => item.ModelId == "priced-chat");
        var second = seeded.ModelConfigurations.Single(item => item.ApiEndpoint == "https://east.example/v1");

        var logs = await RecordAndLoadAsync(seeded, first, second);

        logs.Select(log => log.ModelConfigurationId).Distinct().Count().ShouldBe(2);
        logs.ShouldAllBe(log => log.ModelConfigurationId.HasValue);
    }

    private async Task<List<AIUsageLog>> RecordAndLoadAsync(
        Workspace workspace,
        params AIModelConfiguration[] routes)
    {
        await WithUnitOfWorkAsync(async services =>
        {
            var recorder = services.GetRequiredService<IAIUsageRecorder>();
            foreach (var route in routes)
            {
                await recorder.RecordAsync(CreateSuccessRecord(workspace, route));
            }
        });

        return await WithUnitOfWorkAsync(services =>
            services.GetRequiredService<IAIUsageLogRepository>().GetByWorkspaceAsync(workspace.Id));
    }

    private AIUsageRecord CreateSuccessRecord(Workspace workspace, AIModelConfiguration route)
    {
        return new AIUsageRecord
        {
            Configuration = _runtimeResolver.Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                route,
                isExplicitSelection: true),
            InputTokens = 10,
            OutputTokens = 5,
            LatencyMs = 8,
            IsSuccess = true
        };
    }

    private async Task<Workspace> SeedWorkspaceAsync()
    {
        Guid workspaceId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                Guid.NewGuid(),
                "usage-recorder-" + Guid.NewGuid().ToString("N")[..8],
                AIProviderType.OpenAI,
                "workspace-default");
            workspace.UpdateConfiguration(
                "workspace-default",
                "sk-workspace",
                "https://api.example/v1",
                inputCostPer1MTokens: 1m,
                outputCostPer1MTokens: 2m);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "priced-chat",
                apiEndpoint: "https://api.example/v1",
                apiKey: "sk-priced",
                priority: 0,
                inputCostPer1MTokens: 10m,
                outputCostPer1MTokens: 20m,
                isUserSelectable: true);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "shared-chat",
                apiEndpoint: "https://east.example/v1",
                apiKey: "sk-east",
                priority: 1,
                isUserSelectable: true);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "shared-chat",
                apiEndpoint: "https://west.example/v1",
                apiKey: "sk-west",
                priority: 2,
                isUserSelectable: true);
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            workspaceId = workspace.Id;
        });

        return await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
    }
}
