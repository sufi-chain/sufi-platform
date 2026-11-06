using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareDefinitionWorkspaceBindingTests
{
    [Fact]
    public async Task GetList_keeps_rows_and_flags_a_missing_workspace_binding()
    {
        var missingWorkspaceId = Guid.Parse("9d2df122-424c-34d0-9abc-000000000001");
        var definition = CreateDefinition(missingWorkspaceId, "Forms:Analyst");
        var repository = Substitute.For<IHooshvareDefinitionRepository>();
        repository.GetQueryableAsync()
            .Returns(new[] { definition }.AsQueryable());

        var resolver = Substitute.For<IHooshvareWorkspaceResolver>();
        resolver.FindManyAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var ids = call.Arg<IEnumerable<Guid>>().Distinct();
                return ids.ToDictionary(id => id, _ => (HooshvareWorkspaceBinding?)null);
            });

        var overrideRepository = Substitute.For<IHooshvareDefinitionOverrideRepository>();
        overrideRepository.FindByHooshvareAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((HooshvareDefinitionOverride?)null);

        var service = CreateService(repository, resolver, overrideRepository);

        var result = await service.GetListAsync(new GetHooshvareDefinitionsInput
        {
            MaxResultCount = 100
        });

        result.TotalCount.ShouldBe(1);
        result.Items.ShouldHaveSingleItem();
        result.Items[0].IsWorkspaceAvailable.ShouldBeFalse();
        result.Items[0].WorkspaceId.ShouldBe(missingWorkspaceId);
        result.Items[0].WorkspaceName.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReassignWorkspace_moves_hooshvares_off_a_missing_source()
    {
        var missingWorkspaceId = Guid.Parse("9d2df122-424c-34d0-9abc-000000000001");
        var targetWorkspaceId = Guid.NewGuid();
        var definition = CreateDefinition(missingWorkspaceId, "CRM:Onboarding");
        var repository = Substitute.For<IHooshvareDefinitionRepository>();
        repository.GetQueryableAsync()
            .Returns(new[] { definition }.AsQueryable());
        repository.UpdateAsync(definition, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(definition);

        var resolver = Substitute.For<IHooshvareWorkspaceResolver>();
        resolver.ResolveAsync(targetWorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = targetWorkspaceId,
                WorkspaceName = "Recovered workspace",
                IsActive = true,
                IsReady = true
            });

        var overrideRepository = Substitute.For<IHooshvareDefinitionOverrideRepository>();
        overrideRepository.GetQueryableAsync()
            .Returns(Array.Empty<HooshvareDefinitionOverride>().AsQueryable());

        var service = CreateService(repository, resolver, overrideRepository);

        var result = await service.ReassignWorkspaceAsync(new ReassignHooshvareWorkspaceInput
        {
            SourceWorkspaceId = missingWorkspaceId,
            TargetWorkspaceId = targetWorkspaceId
        });

        result.UpdatedCount.ShouldBe(1);
        definition.WorkspaceId.ShouldBe(targetWorkspaceId);
        await repository.Received(1)
            .UpdateAsync(definition, autoSave: true, Arg.Any<CancellationToken>());
    }

    private static HooshvareDefinitionAppService CreateService(
        IHooshvareDefinitionRepository repository,
        IHooshvareWorkspaceResolver resolver,
        IHooshvareDefinitionOverrideRepository overrideRepository)
    {
        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>(), resourceNotFound: true));
        var localizerFactory = Substitute.For<IStringLocalizerFactory>();
        localizerFactory.Create(Arg.Any<Type>()).Returns(localizer);
        localizerFactory.Create(Arg.Any<string>(), Arg.Any<string>()).Returns(localizer);

        var asyncExecuter = Substitute.For<IAsyncQueryableExecuter>();
        asyncExecuter.ToListAsync(Arg.Any<IQueryable<HooshvareDefinition>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IQueryable<HooshvareDefinition>>().ToList());
        asyncExecuter.ToListAsync(Arg.Any<IQueryable<HooshvareDefinitionOverride>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IQueryable<HooshvareDefinitionOverride>>().ToList());

        var services = new ServiceCollection();
        services.AddSingleton(asyncExecuter);

        var service = new HooshvareDefinitionAppService(
            repository,
            new HooshvareDefinitionManager(),
            new HooshvareBusinessLocalizationService(localizerFactory),
            resolver,
            overrideRepository,
            new HooshvareEffectiveConfigurationResolver(
                overrideRepository,
                Substitute.For<IHooshvareRagProjectBindingRepository>(),
                resolver),
            Substitute.For<ISettingsStore>(),
            Substitute.For<IMCPToolRegistry>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static HooshvareDefinition CreateDefinition(Guid workspaceId, string key)
    {
        return new HooshvareDefinition(
            Guid.NewGuid(),
            tenantId: Guid.NewGuid(),
            sourceModule: "SufiForms",
            displayName: "List test hooshvare",
            kind: HooshvareKind.Assistant,
            purpose: "Test",
            workspaceId: workspaceId,
            systemPrompt: "Test prompt",
            persistChatSession: false,
            key: key,
            isStatic: false);
    }
}
