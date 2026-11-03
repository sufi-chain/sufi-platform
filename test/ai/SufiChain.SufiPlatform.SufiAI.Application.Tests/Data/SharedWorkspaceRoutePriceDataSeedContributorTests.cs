using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Data;

public class SharedWorkspaceRoutePriceDataSeedContributorTests
{
    [Fact]
    public async Task Host_seed_prices_a_zero_chat_route_on_the_shared_workspace_only()
    {
        var shared = Workspace("زیرساخت صوفی", marked: true);
        var route = shared.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "openrouter/free",
            inputPrice: 0m,
            outputPrice: 0m);
        var other = Workspace("tenant-byok", marked: false);
        var otherRoute = other.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "other-model",
            inputPrice: 0m,
            outputPrice: 0m);
        var repository = Repository(shared, other);
        var contributor = Contributor(repository, Schedule(1m, 2m));

        await contributor.SeedAsync(new DataSeedContext(null));

        route.InputPrice.ShouldBe(1m);
        route.OutputPrice.ShouldBe(2m);
        otherRoute.InputPrice.ShouldBe(0m);
        await repository.Received(1).UpdateAsync(shared, true, Arg.Any<CancellationToken>());
        await repository.DidNotReceive().UpdateAsync(other, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Catalog_price_wins_over_the_host_floor()
    {
        var shared = Workspace("shared", marked: true);
        var route = shared.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "qwen/qwen-2.5-72b-instruct",
            inputPrice: 0m,
            outputPrice: 0m);
        var catalog = Substitute.For<IModelCatalogSource>();
        catalog.GetModelsAsync("OpenRouter", Arg.Any<CancellationToken>()).Returns(
            new List<ModelCatalogEntry>
            {
                new()
                {
                    Id = route.ModelId,
                    PromptPricePerToken = 0.00000036m,
                    CompletionPricePerToken = 0.0000004m
                }
            });
        var profile = Substitute.For<IAiProviderProfile>();
        profile.ProviderType.Returns(AIProviderType.OpenRouter);
        profile.CatalogName.Returns("OpenRouter");
        var contributor = Contributor(
            Repository(shared),
            Schedule(9m, 9m),
            catalog,
            new[] { profile });

        await contributor.SeedAsync(new DataSeedContext(null));

        route.InputPrice.ShouldBe(0.36m);
        route.OutputPrice.ShouldBe(0.4m);
    }

    [Fact]
    public async Task Stored_positive_price_is_not_replaced_by_the_floor()
    {
        var shared = Workspace("shared", marked: true);
        var route = shared.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "administrator-chat",
            inputPrice: 2.5m,
            outputPrice: 4m);
        var repository = Repository(shared);
        var contributor = Contributor(repository, Schedule(1m, 2m));

        await contributor.SeedAsync(new DataSeedContext(null));

        route.InputPrice.ShouldBe(2.5m);
        route.OutputPrice.ShouldBe(4m);
        await repository.DidNotReceive().UpdateAsync(
            Arg.Any<Workspace>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unavailable_catalog_still_applies_the_floor()
    {
        var shared = Workspace("shared", marked: true);
        var route = shared.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "openrouter/free",
            inputPrice: 0m,
            outputPrice: 0m);
        var catalog = Substitute.For<IModelCatalogSource>();
        catalog.GetModelsAsync("OpenRouter", Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<ModelCatalogEntry>?>>(_ => throw new InvalidOperationException("catalog down"));
        var profile = Substitute.For<IAiProviderProfile>();
        profile.ProviderType.Returns(AIProviderType.OpenRouter);
        profile.CatalogName.Returns("OpenRouter");
        var contributor = Contributor(Repository(shared), Schedule(1m, 2m), catalog, new[] { profile });

        await contributor.SeedAsync(new DataSeedContext(null));

        route.InputPrice.ShouldBe(1m);
        route.OutputPrice.ShouldBe(2m);
    }

    [Fact]
    public async Task Inherited_workspace_is_not_priced_by_this_seed()
    {
        var inherited = Workspace("زیرساخت صوفی", marked: true, tenantId: Guid.NewGuid());
        inherited.MarkAsInherited(Guid.NewGuid(), Guid.NewGuid());
        var route = inherited.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "openrouter/free",
            inputPrice: 0m,
            outputPrice: 0m);
        var repository = Repository(inherited);
        var contributor = Contributor(repository, Schedule(1m, 2m));

        await contributor.SeedAsync(new DataSeedContext(null));

        route.InputPrice.ShouldBe(0m);
        await repository.DidNotReceive().UpdateAsync(
            Arg.Any<Workspace>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Tenant_seed_does_not_price_the_host_workspace()
    {
        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        var repository = Substitute.For<IWorkspaceRepository>();
        var contributor = Contributor(repository, Schedule(1m, 2m), workspaceSeeder: seeder);

        await contributor.SeedAsync(new DataSeedContext(Guid.NewGuid()));

        await seeder.DidNotReceive().EnsureDefaultWorkspaceAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive().GetCountAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ambient_tenant_does_not_price_the_host_workspace()
    {
        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        var contributor = Contributor(
            Substitute.For<IWorkspaceRepository>(),
            Schedule(1m, 2m),
            workspaceSeeder: seeder,
            tenantId: Guid.NewGuid());

        await contributor.SeedAsync(new DataSeedContext(null));

        await seeder.DidNotReceive().EnsureDefaultWorkspaceAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    private static SharedWorkspaceRoutePriceSchedule Schedule(decimal input, decimal output)
    {
        return new SharedWorkspaceRoutePriceSchedule
        {
            ChatInputPerMillionTokens = input,
            ChatOutputPerMillionTokens = output
        };
    }

    private static Workspace Workspace(string name, bool marked, Guid? tenantId = null)
    {
        var workspace = new Workspace(Guid.NewGuid(), name, AIProviderType.OpenRouter, "openrouter/free", tenantId);
        if (marked)
        {
            HostDefaultWorkspaceMarker.Mark(workspace);
        }

        return workspace;
    }

    private static IWorkspaceRepository Repository(params Workspace[] workspaces)
    {
        var repository = Substitute.For<IWorkspaceRepository>();
        repository.GetCountAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(workspaces.LongLength);
        repository.GetListAsync(
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(workspaces.ToList());
        repository.UpdateAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Workspace>());
        return repository;
    }

    private static SharedWorkspaceRoutePriceDataSeedContributor Contributor(
        IWorkspaceRepository repository,
        SharedWorkspaceRoutePriceSchedule schedule,
        IModelCatalogSource? catalog = null,
        IEnumerable<IAiProviderProfile>? profiles = null,
        IDefaultAiWorkspaceSeeder? workspaceSeeder = null,
        Guid? tenantId = null)
    {
        var prices = Substitute.For<ISharedWorkspaceRoutePriceSource>();
        prices.GetAsync().Returns(schedule);
        var seeder = workspaceSeeder ?? Substitute.For<IDefaultAiWorkspaceSeeder>();
        return new SharedWorkspaceRoutePriceDataSeedContributor(
            seeder,
            repository,
            CurrentTenant(tenantId),
            prices,
            catalog ?? Substitute.For<IModelCatalogSource>(),
            new HostPriceMarkup(null!, null!),
            profiles ?? Array.Empty<IAiProviderProfile>(),
            Options.Create(new AIOptions
            {
                DefaultWorkspace = new DefaultWorkspaceSeedOptions { Name = "زیرساخت صوفی" }
            }),
            NullLogger<SharedWorkspaceRoutePriceDataSeedContributor>.Instance);
    }

    private static ICurrentTenant CurrentTenant(Guid? tenantId)
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        currentTenant.Change(Arg.Any<Guid?>()).Returns(new DisposeAction());
        return currentTenant;
    }

    private sealed class DisposeAction : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
