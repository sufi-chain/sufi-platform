using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Data;

public class DefaultAiWorkspaceSeederTests
{
    [Fact]
    public async Task Should_Keep_Administrator_Fields_And_Add_Only_Missing_Managed_Capabilities()
    {
        var repository = Substitute.For<IWorkspaceRepository>();
        var existing = new Workspace(
            Guid.NewGuid(),
            AIWorkspaceNames.Default,
            AIProviderType.OpenAI,
            "administrator-model");
        existing.AddModelConfiguration(AICapabilityType.ChatCompletion, "administrator-chat");
        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns(existing);

        var seeder = CreateSeeder(repository, new DefaultWorkspaceSeedOptions
        {
            Model = "seed-chat",
            EmbeddingModel = "seed-embedding"
        });

        var workspaceId = await seeder.EnsureDefaultWorkspaceAsync();

        workspaceId.ShouldBe(existing.Id);
        existing.DefaultModel.ShouldBe("administrator-model");
        existing.ModelConfigurations.Count.ShouldBe(2);
        existing.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.ModelId.ShouldBe("administrator-chat");
        existing.GetPrimaryConfiguration(AICapabilityType.Embeddings)!.ModelId.ShouldBe("seed-embedding");
        await repository.Received(1)
            .UpdateAsync(existing, Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive()
            .InsertAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Change_A_Reconciled_Existing_Workspace()
    {
        var repository = Substitute.For<IWorkspaceRepository>();
        var existing = new Workspace(
            Guid.NewGuid(),
            AIWorkspaceNames.Default,
            AIProviderType.OpenAI,
            "administrator-model");
        existing.AddModelConfiguration(AICapabilityType.ChatCompletion, "administrator-chat");
        existing.AddModelConfiguration(AICapabilityType.Embeddings, "administrator-embedding");
        existing.SetProperty(
            DefaultWorkspaceManagedFieldReconciler.SeedVersionProperty,
            DefaultWorkspaceManagedFieldReconciler.CurrentSeedVersion);
        HostDefaultWorkspaceMarker.Mark(existing);
        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns(existing);

        var seeder = CreateSeeder(repository, new DefaultWorkspaceSeedOptions
        {
            Model = "seed-chat",
            EmbeddingModel = "seed-embedding"
        });

        var workspaceId = await seeder.EnsureDefaultWorkspaceAsync();

        workspaceId.ShouldBe(existing.Id);
        existing.ModelConfigurations.Count.ShouldBe(2);
        existing.GetPrimaryConfiguration(AICapabilityType.Embeddings)!.ModelId.ShouldBe("administrator-embedding");
        await repository.DidNotReceive()
            .UpdateAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive()
            .InsertAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Create_Only_Explicitly_Configured_Capabilities()
    {
        var repository = Substitute.For<IWorkspaceRepository>();
        Workspace? inserted = null;
        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns((Workspace?)null);
        repository.InsertAsync(
                Arg.Do<Workspace>(workspace => inserted = workspace),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Workspace>());

        var seeder = CreateSeeder(repository, new DefaultWorkspaceSeedOptions
        {
            Model = "chat-model",
            EmbeddingModel = "embedding-model",
            VisionModel = "vision-model",
            AudioModel = "",
            TtsModel = "",
            ImageModel = ""
        });

        await seeder.EnsureDefaultWorkspaceAsync();

        inserted.ShouldNotBeNull();
        inserted.TenantId.ShouldBeNull();
        inserted.ModelConfigurations
            .Select(configuration => configuration.CapabilityType)
            .ShouldBe(new[]
            {
                AICapabilityType.ChatCompletion,
                AICapabilityType.Embeddings,
                AICapabilityType.VisionAnalysis
            });
        inserted.ModelConfigurations[0].OpenAIApiMode.ShouldBe(OpenAIApiMode.ChatCompletions);
        HostDefaultWorkspaceMarker.IsMarked(inserted).ShouldBeTrue();
        inserted.ModelConfigurations[0].MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }

    [Fact]
    public async Task Should_Not_Mutate_Inherited_Default_Workspace()
    {
        var tenantId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var existing = new Workspace(
            Guid.NewGuid(),
            AIWorkspaceNames.Default,
            AIProviderType.OpenAI,
            "host-model",
            tenantId);
        existing.MarkAsInherited(Guid.NewGuid(), Guid.NewGuid());
        existing.AddModelConfiguration(AICapabilityType.ChatCompletion, "host-chat");
        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns(existing);

        var seeder = CreateSeeder(
            repository,
            new DefaultWorkspaceSeedOptions
            {
                Model = "seed-chat",
                EmbeddingModel = "seed-embedding"
            },
            tenantId);

        var workspaceId = await seeder.EnsureDefaultWorkspaceAsync();

        workspaceId.ShouldBe(existing.Id);
        existing.ModelConfigurations.Count.ShouldBe(1);
        await repository.DidNotReceive()
            .UpdateAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await repository.DidNotReceive()
            .InsertAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Inherit_Host_Default_For_Tenant_Instead_Of_Seeding_Dedicated()
    {
        var tenantId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var assignments = Substitute.For<IWorkspaceAssignmentRepository>();
        var synchronizer = Substitute.For<IInheritedWorkspaceProjectionSynchronizer>();

        var hostWorkspace = new Workspace(
            hostWorkspaceId,
            AIWorkspaceNames.Default,
            AIProviderType.OpenAI,
            "host-model");
        var projection = new Workspace(
            projectionId,
            AIWorkspaceNames.Default,
            AIProviderType.OpenAI,
            "host-model",
            tenantId);
        projection.MarkAsInherited(hostWorkspaceId, Guid.NewGuid());

        // active + soft-delete probes before sync, again after sync, then host lookup.
        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns(
                (Workspace?)null,
                (Workspace?)null,
                (Workspace?)null,
                (Workspace?)null,
                hostWorkspace);
        repository.UpdateAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Workspace>());

        assignments.FindAsync(tenantId, hostWorkspaceId, Arg.Any<CancellationToken>())
            .Returns((WorkspaceAssignment?)null);
        assignments.InsertAsync(Arg.Any<WorkspaceAssignment>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<WorkspaceAssignment>());

        synchronizer.CreateTenantProjectionAsync(
                hostWorkspace,
                tenantId,
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                hostWorkspaceId,
                AIWorkspaceNames.Default,
                Arg.Any<CancellationToken>())
            .Returns(projection);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(unitOfWork);

        var seeder = CreateSeeder(
            repository,
            new DefaultWorkspaceSeedOptions { Model = "seed-chat" },
            tenantId,
            assignments,
            synchronizer,
            unitOfWorkManager);

        var workspaceId = await seeder.EnsureDefaultWorkspaceAsync();

        workspaceId.ShouldBe(projectionId);
        await repository.DidNotReceive()
            .InsertAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await synchronizer.Received(1).EnsureCurrentTenantAsync(Arg.Any<CancellationToken>());
        await assignments.Received(1)
            .InsertAsync(Arg.Any<WorkspaceAssignment>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        unitOfWorkManager.Received(1).Begin(
            Arg.Is<AbpUnitOfWorkOptions>(options => options.IsTransactional == false),
            true);
        await unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Inherit_Renamed_Host_Workspace_Marked_By_Seed_Version()
    {
        var tenantId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var repository = Substitute.For<IWorkspaceRepository>();
        var assignments = Substitute.For<IWorkspaceAssignmentRepository>();
        var synchronizer = Substitute.For<IInheritedWorkspaceProjectionSynchronizer>();
        var hostWorkspace = new Workspace(
            hostWorkspaceId,
            "زیرساخت صوفی",
            AIProviderType.OpenRouter,
            "openrouter/free");
        hostWorkspace.SetProperty(
            DefaultWorkspaceManagedFieldReconciler.SeedVersionProperty,
            DefaultWorkspaceManagedFieldReconciler.CurrentSeedVersion);
        var projection = new Workspace(
            projectionId,
            hostWorkspace.Name,
            AIProviderType.OpenRouter,
            "openrouter/free",
            tenantId);
        projection.MarkAsInherited(hostWorkspaceId, Guid.NewGuid());

        repository.FindByNameAsync(AIWorkspaceNames.Default, Arg.Any<CancellationToken>())
            .Returns((Workspace?)null);
        repository.GetCountAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(1L);
        repository.GetListAsync(
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<Workspace> { hostWorkspace });
        repository.UpdateAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Workspace>());
        assignments.FindAsync(tenantId, hostWorkspaceId, Arg.Any<CancellationToken>())
            .Returns((WorkspaceAssignment?)null);
        assignments.InsertAsync(Arg.Any<WorkspaceAssignment>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<WorkspaceAssignment>());
        synchronizer.CreateTenantProjectionAsync(
                hostWorkspace,
                tenantId,
                Arg.Any<Guid>(),
                Arg.Any<Guid>(),
                hostWorkspaceId,
                hostWorkspace.Name,
                Arg.Any<CancellationToken>())
            .Returns(projection);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(unitOfWork);

        var seeder = CreateSeeder(
            repository,
            new DefaultWorkspaceSeedOptions { Name = AIWorkspaceNames.Default, Model = "seed-chat" },
            tenantId,
            assignments,
            synchronizer,
            unitOfWorkManager);

        var workspaceId = await seeder.EnsureDefaultWorkspaceAsync();

        workspaceId.ShouldBe(projectionId);
        HostDefaultWorkspaceMarker.IsMarked(hostWorkspace).ShouldBeTrue();
        await repository.DidNotReceive()
            .InsertAsync(Arg.Any<Workspace>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Selects_IsDefault_ahead_of_the_configured_name()
    {
        var named = new Workspace(Guid.NewGuid(), AIWorkspaceNames.Default, AIProviderType.OpenAI, "named");
        var marked = new Workspace(Guid.NewGuid(), "زیرساخت صوفی", AIProviderType.OpenRouter, "marked");
        HostDefaultWorkspaceMarker.Mark(marked);

        HostDefaultWorkspaceMarker.Select(new[] { named, marked }, AIWorkspaceNames.Default)!.Id.ShouldBe(marked.Id);
    }

    private static DefaultAiWorkspaceSeeder CreateSeeder(
        IWorkspaceRepository repository,
        DefaultWorkspaceSeedOptions seedOptions,
        Guid? tenantId = null,
        IWorkspaceAssignmentRepository? assignments = null,
        IInheritedWorkspaceProjectionSynchronizer? synchronizer = null,
        IUnitOfWorkManager? unitOfWorkManager = null)
    {
        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(_ => Guid.NewGuid());

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        currentTenant.Change(Arg.Any<Guid?>())
            .Returns(ci =>
            {
                var previous = currentTenant.Id;
                currentTenant.Id.Returns(ci.Arg<Guid?>());
                return new DisposeAction(() => currentTenant.Id.Returns(previous));
            });

        var dataFilter = Substitute.For<IDataFilter>();
        dataFilter.Disable<ISoftDelete>().Returns(new DisposeAction(() => { }));
        dataFilter.Disable<IMultiTenant>().Returns(new DisposeAction(() => { }));

        if (unitOfWorkManager == null)
        {
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
            unitOfWorkManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(unitOfWork);
        }

        return new DefaultAiWorkspaceSeeder(
            repository,
            assignments ?? Substitute.For<IWorkspaceAssignmentRepository>(),
            synchronizer ?? Substitute.For<IInheritedWorkspaceProjectionSynchronizer>(),
            guidGenerator,
            currentTenant,
            dataFilter,
            unitOfWorkManager,
            Substitute.For<IStringEncryptionService>(),
            Options.Create(new AIOptions
            {
                SeedDefaultWorkspace = true,
                DefaultWorkspace = seedOptions
            }),
            NullLogger<DefaultAiWorkspaceSeeder>.Instance);
    }

    private sealed class DisposeAction : IDisposable
    {
        private readonly Action _action;

        public DisposeAction(Action action) => _action = action;

        public void Dispose() => _action();
    }
}
