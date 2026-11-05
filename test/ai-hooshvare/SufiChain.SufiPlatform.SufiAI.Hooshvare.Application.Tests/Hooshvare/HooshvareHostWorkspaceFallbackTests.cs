using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareHostWorkspaceFallbackTests
{
    [Fact]
    public async Task Resolve_Should_Use_The_Host_Default_When_The_Bound_Workspace_Is_Missing()
    {
        var hostWorkspaceId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SufiAIWorkspaceDescriptor?)null);
        catalog.FindByIdAsync(hostWorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new SufiAIWorkspaceDescriptor
            {
                Id = hostWorkspaceId,
                Name = AIWorkspaceNames.Default,
                IsActive = true,
                IsReady = true
            });

        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        seeder.EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>()).Returns(hostWorkspaceId);

        var resolver = CreateResolver(catalog, seeder, tenantId: null);

        var binding = await resolver.ResolveAsync(Guid.NewGuid());

        binding.WorkspaceId.ShouldBe(hostWorkspaceId);
        await seeder.Received(1).EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resolve_Should_Inherit_The_Host_Default_When_A_Tenant_Cannot_See_The_Bound_Workspace()
    {
        var tenantId = Guid.NewGuid();
        var inheritedId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SufiAIWorkspaceDescriptor?)null);
        catalog.FindByIdAsync(inheritedId, Arg.Any<CancellationToken>())
            .Returns(new SufiAIWorkspaceDescriptor
            {
                Id = inheritedId,
                Name = "Inherited default",
                IsActive = true,
                IsReady = true,
                IsInherited = true
            });

        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        seeder.EnsureDefaultWorkspaceAsync(tenantId, Arg.Any<CancellationToken>()).Returns(inheritedId);
        var resolver = CreateResolver(catalog, seeder, tenantId);

        var binding = await resolver.ResolveAsync(Guid.NewGuid());

        binding.WorkspaceId.ShouldBe(inheritedId);
        binding.WorkspaceName.ShouldBe("Inherited default");
        await seeder.Received(1).EnsureDefaultWorkspaceAsync(tenantId, Arg.Any<CancellationToken>());
        await seeder.DidNotReceive().EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resolve_Should_Keep_A_Tenant_Workspace_The_Tenant_Can_Already_See()
    {
        var tenantId = Guid.NewGuid();
        var ownId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindByIdAsync(ownId, Arg.Any<CancellationToken>())
            .Returns(new SufiAIWorkspaceDescriptor
            {
                Id = ownId,
                Name = "Tenant workspace",
                IsActive = true,
                IsReady = true
            });
        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        var resolver = CreateResolver(catalog, seeder, tenantId);

        var binding = await resolver.ResolveAsync(ownId);

        binding.WorkspaceId.ShouldBe(ownId);
        binding.WorkspaceName.ShouldBe("Tenant workspace");
        await seeder.DidNotReceive().EnsureDefaultWorkspaceAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await seeder.DidNotReceive().EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resolve_Should_Fail_When_A_Tenant_Has_No_Inherited_Host_Default()
    {
        var tenantId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SufiAIWorkspaceDescriptor?)null);
        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        seeder.EnsureDefaultWorkspaceAsync(tenantId, Arg.Any<CancellationToken>()).Returns((Guid?)null);
        var resolver = CreateResolver(catalog, seeder, tenantId);

        var exception = await Should.ThrowAsync<BusinessException>(() => resolver.ResolveAsync(Guid.NewGuid()));

        exception.Code.ShouldBe(AIHooshvareErrorCodes.WorkspaceNotFound);
    }

    [Fact]
    public async Task Seed_Should_Rebind_A_Host_Hooshvare_Whose_Workspace_Is_Outside_Host_Scope()
    {
        var tenantWorkspaceId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();
        var definition = CreateStaticDefinition(tenantId: null, tenantWorkspaceId);
        var repository = Substitute.For<IHooshvareDefinitionRepository>();
        repository.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new List<HooshvareDefinition> { definition });
        repository.UpdateAsync(definition, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(definition);

        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.FindByIdAsync(tenantWorkspaceId, Arg.Any<CancellationToken>())
            .Returns((SufiAIWorkspaceDescriptor?)null);

        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        seeder.EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>()).Returns(hostWorkspaceId);

        var contributor = new PlatformHooshvareWorkspaceAlignmentDataSeedContributor(
            repository,
            CreateCurrentTenant(null),
            seeder,
            catalog,
            NullLogger<PlatformHooshvareWorkspaceAlignmentDataSeedContributor>.Instance);

        await contributor.SeedAsync(new Volo.Abp.Data.DataSeedContext(null));

        definition.WorkspaceId.ShouldBe(hostWorkspaceId);
    }

    [Fact]
    public async Task Seed_Should_Keep_A_Tenant_Hooshvare_Workspace_That_Is_Already_Set()
    {
        var tenantId = Guid.NewGuid();
        var assignedWorkspaceId = Guid.NewGuid();
        var definition = CreateStaticDefinition(tenantId, assignedWorkspaceId);
        var repository = Substitute.For<IHooshvareDefinitionRepository>();
        repository.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new List<HooshvareDefinition> { definition });

        var seeder = Substitute.For<IDefaultAiWorkspaceSeeder>();
        seeder.EnsureDefaultWorkspaceAsync(Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        var contributor = new PlatformHooshvareWorkspaceAlignmentDataSeedContributor(
            repository,
            CreateCurrentTenant(tenantId),
            seeder,
            Substitute.For<ISufiAIWorkspaceCatalog>(),
            NullLogger<PlatformHooshvareWorkspaceAlignmentDataSeedContributor>.Instance);

        await contributor.SeedAsync(new Volo.Abp.Data.DataSeedContext(tenantId));

        definition.WorkspaceId.ShouldBe(assignedWorkspaceId);
        await repository.DidNotReceive()
            .UpdateAsync(Arg.Any<HooshvareDefinition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    private static HooshvareWorkspaceResolver CreateResolver(
        ISufiAIWorkspaceCatalog catalog,
        IDefaultAiWorkspaceSeeder seeder,
        Guid? tenantId)
    {
        return new HooshvareWorkspaceResolver(
            catalog,
            seeder,
            CreateCurrentTenant(tenantId),
            Options.Create(new AIOptions
            {
                DefaultWorkspace = new DefaultWorkspaceSeedOptions
                {
                    Name = AIWorkspaceNames.Default
                }
            }),
            NullLogger<HooshvareWorkspaceResolver>.Instance);
    }

    private static HooshvareDefinition CreateStaticDefinition(Guid? tenantId, Guid workspaceId)
    {
        return new HooshvareDefinition(
            Guid.NewGuid(),
            tenantId,
            sourceModule: "SufiIdentity",
            displayName: "Admin Advisor",
            kind: HooshvareKind.Assistant,
            purpose: "Advisor",
            workspaceId: workspaceId,
            systemPrompt: "Advise the administrator.",
            persistChatSession: false,
            key: "SufiIdentity:AdminAdvisor",
            isStatic: true);
    }

    private static ICurrentTenant CreateCurrentTenant(Guid? tenantId)
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        currentTenant.Change(Arg.Any<Guid?>())
            .Returns(call =>
            {
                var previous = currentTenant.Id;
                currentTenant.Id.Returns(call.Arg<Guid?>());
                return new RestoreTenant(() => currentTenant.Id.Returns(previous));
            });
        return currentTenant;
    }

    private sealed class RestoreTenant : IDisposable
    {
        private readonly Action _restore;

        public RestoreTenant(Action restore) => _restore = restore;

        public void Dispose() => _restore();
    }
}
