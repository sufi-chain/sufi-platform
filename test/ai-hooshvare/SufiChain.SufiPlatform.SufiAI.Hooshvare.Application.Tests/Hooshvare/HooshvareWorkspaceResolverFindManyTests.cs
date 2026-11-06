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
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareWorkspaceResolverFindManyTests
{
    [Fact]
    public async Task FindMany_marks_a_missing_workspace_as_null_without_throwing()
    {
        var missingId = Guid.Parse("9d2df122-424c-34d0-9abc-000000000001");
        var presentId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.GetListAsync(Arg.Any<CancellationToken>())
            .Returns(new List<SufiAIWorkspaceDescriptor>
            {
                new()
                {
                    Id = presentId,
                    Name = "Active workspace",
                    IsActive = true,
                    IsReady = true
                }
            });

        var resolver = CreateResolver(catalog);

        var bindings = await resolver.FindManyAsync([missingId, presentId, Guid.Empty]);

        bindings[missingId].ShouldBeNull();
        bindings[Guid.Empty].ShouldBeNull();
        bindings[presentId].ShouldNotBeNull();
        bindings[presentId]!.WorkspaceName.ShouldBe("Active workspace");
        await catalog.DidNotReceive()
            .FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FindMany_returns_null_bindings_when_the_catalog_list_fails()
    {
        var workspaceId = Guid.NewGuid();
        var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
        catalog.GetListAsync(Arg.Any<CancellationToken>())
            .Returns<Task<List<SufiAIWorkspaceDescriptor>>>(_ =>
                throw new InvalidOperationException("catalog unavailable"));

        var resolver = CreateResolver(catalog);

        var bindings = await resolver.FindManyAsync([workspaceId]);

        bindings[workspaceId].ShouldBeNull();
    }

    private static HooshvareWorkspaceResolver CreateResolver(ISufiAIWorkspaceCatalog catalog)
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);

        return new HooshvareWorkspaceResolver(
            catalog,
            Substitute.For<IDefaultAiWorkspaceSeeder>(),
            currentTenant,
            Options.Create(new AIOptions
            {
                DefaultWorkspace = new DefaultWorkspaceSeedOptions
                {
                    Name = AIWorkspaceNames.Default
                }
            }),
            NullLogger<HooshvareWorkspaceResolver>.Instance);
    }
}
