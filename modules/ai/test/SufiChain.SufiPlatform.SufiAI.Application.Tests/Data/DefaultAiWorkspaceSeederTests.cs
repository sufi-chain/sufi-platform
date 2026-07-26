using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Data;

public class DefaultAiWorkspaceSeederTests
{
    [Fact]
    public async Task Should_Not_Change_Existing_Workspace()
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
        existing.ModelConfigurations.Count.ShouldBe(1);
        existing.ModelConfigurations[0].ModelId.ShouldBe("administrator-chat");
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
        inserted.ModelConfigurations
            .Select(configuration => configuration.CapabilityType)
            .ShouldBe(new[]
            {
                AICapabilityType.ChatCompletion,
                AICapabilityType.Embeddings,
                AICapabilityType.VisionAnalysis
            });
        inserted.SystemPrompt.ShouldBeNull();
    }

    private static DefaultAiWorkspaceSeeder CreateSeeder(
        IWorkspaceRepository repository,
        DefaultWorkspaceSeedOptions seedOptions)
    {
        var guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(Guid.NewGuid());

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns((Guid?)null);

        return new DefaultAiWorkspaceSeeder(
            repository,
            guidGenerator,
            currentTenant,
            Substitute.For<IStringEncryptionService>(),
            Options.Create(new AIOptions
            {
                SeedDefaultWorkspace = true,
                DefaultWorkspace = seedOptions
            }),
            NullLogger<DefaultAiWorkspaceSeeder>.Instance);
    }
}
