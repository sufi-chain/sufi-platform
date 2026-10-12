using System;
using System.Linq;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Workspaces;

public class WorkspaceTests : SufiAITestBase<SufiAIDomainTestModule>
{
    [Fact]
    public void Should_Create_Workspace_With_Valid_Data()
    {
        // Arrange & Act
        var workspace = new Workspace(
            AITestData.Workspaces.DefaultWorkspaceId,
            AITestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            AITestData.Workspaces.DefaultModelId
        );

        // Assert
        workspace.Id.ShouldBe(AITestData.Workspaces.DefaultWorkspaceId);
        workspace.Name.ShouldBe(AITestData.Workspaces.DefaultWorkspaceName);
        workspace.Provider.ShouldBe(AIProviderType.OpenAI);
        workspace.Model.ShouldBe(AITestData.Workspaces.DefaultModelId);
        workspace.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Update_Workspace_Configuration()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-3.5-turbo"
        );

        // Act
        workspace.UpdateConfiguration(
            "gpt-4",
            "new-api-key",
            "https://custom.openai.com/v1"
        );

        // Assert
        workspace.Model.ShouldBe("gpt-4");
        workspace.ApiKey.ShouldBe("new-api-key");
        workspace.ApiBaseUrl.ShouldBe("https://custom.openai.com/v1");
        workspace.Provider.ShouldBe(AIProviderType.OpenAI);
    }

    [Fact]
    public void Should_Change_Provider()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        workspace.SetProvider(AIProviderType.OpenRouter);

        workspace.Provider.ShouldBe(AIProviderType.OpenRouter);
    }

    [Fact]
    public void Should_Activate_And_Deactivate_Workspace()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        // Act & Assert - Deactivate
        workspace.Deactivate();
        workspace.IsActive.ShouldBeFalse();

        // Act & Assert - Activate
        workspace.Activate();
        workspace.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_For_Invalid_Name(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Workspace(
                Guid.NewGuid(),
                invalidName!,
                AIProviderType.OpenAI,
                "gpt-4"
            );
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_For_Invalid_ModelId(string? invalidModelId)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Workspace(
                Guid.NewGuid(),
                "test",
                AIProviderType.OpenAI,
                invalidModelId!
            );
        });
    }

    [Fact]
    public void Should_Keep_Primary_Chat_Route_Price_When_Updating_Workspace()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-3.5-turbo"
        );
        var configuration = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "gpt-3.5-turbo",
            inputPrice: 1.5m,
            outputPrice: 2.5m);

        workspace.UpdateConfiguration(
            "gpt-4",
            "key",
            "https://workspace.example/v1",
            inputCostPer1MTokens: 9m,
            outputCostPer1MTokens: 11m);
        workspace.UpdatePrimaryChatConfiguration(
            "gpt-4",
            "https://workspace.example/v1",
            displayName: null);

        configuration.ModelId.ShouldBe("gpt-4");
        configuration.ApiEndpoint.ShouldBe("https://workspace.example/v1");
        configuration.InputPrice.ShouldBe(1.5m);
        configuration.OutputPrice.ShouldBe(2.5m);
        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }

    [Fact]
    public void Should_Treat_Zero_Max_Context_Tokens_As_Default()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-4"
        );
        var configuration = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "gpt-4");

        configuration.UpdateConfiguration(
            "gpt-4",
            apiEndpoint: null,
            apiKey: null,
            priority: 0,
            maxContextTokens: 0);

        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
        AIModelConfiguration.NormalizeMaxContextTokens(0).ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }

    [Fact]
    public void Append_assigns_contiguous_priorities()
    {
        var workspace = CreateWorkspace();
        var first = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "model-a");
        var second = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "model-b");

        first.Priority.ShouldBe(0);
        second.Priority.ShouldBe(1);
        workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.Id.ShouldBe(first.Id);
    }

    [Fact]
    public void Insert_at_zero_collides_and_shifts_existing()
    {
        var workspace = CreateWorkspace();
        var first = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "model-a");
        var inserted = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "model-b",
            priority: 0);

        inserted.Priority.ShouldBe(0);
        first.Priority.ShouldBe(1);
        workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.Id.ShouldBe(inserted.Id);
    }

    [Fact]
    public void Move_clamps_and_rewrites_priorities()
    {
        var workspace = CreateWorkspace();
        var a = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "a");
        var b = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "b");
        var c = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "c");

        workspace.MoveModelConfiguration(c.Id, position: 0);

        c.Priority.ShouldBe(0);
        a.Priority.ShouldBe(1);
        b.Priority.ShouldBe(2);

        workspace.MoveModelConfiguration(c.Id, position: 99);
        c.Priority.ShouldBe(2);
    }

    [Fact]
    public void Remove_closes_the_priority_gap()
    {
        var workspace = CreateWorkspace();
        var a = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "a");
        var b = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "b");
        var c = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "c");

        workspace.RemoveModelConfiguration(b.Id);

        workspace.ModelConfigurations.Count(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .ShouldBe(2);
        a.Priority.ShouldBe(0);
        c.Priority.ShouldBe(1);
    }

    [Theory]
    [InlineData(AICapabilityType.ChatCompletion)]
    [InlineData(AICapabilityType.Embeddings)]
    [InlineData(AICapabilityType.Decisions)]
    public void Set_default_moves_to_zero_for_any_capability_without_negatives(
        AICapabilityType capability)
    {
        var workspace = CreateWorkspace();
        var first = workspace.AddModelConfiguration(capability, "first");
        var second = workspace.AddModelConfiguration(capability, "second");

        workspace.SetDefaultModelConfiguration(second.Id);

        second.Priority.ShouldBe(0);
        first.Priority.ShouldBe(1);
        second.Priority.ShouldBeGreaterThanOrEqualTo(0);
        first.Priority.ShouldBeGreaterThanOrEqualTo(0);
        workspace.GetPrimaryConfiguration(capability)!.Id.ShouldBe(second.Id);

        if (capability == AICapabilityType.ChatCompletion)
        {
            workspace.DefaultModel.ShouldBe("second");
        }
    }

    [Fact]
    public void Change_capability_resequences_both_groups()
    {
        var workspace = CreateWorkspace();
        var chatA = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "chat-a");
        var chatB = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "chat-b");
        var embed = workspace.AddModelConfiguration(AICapabilityType.Embeddings, "embed-a");

        workspace.ChangeCapability(chatB.Id, AICapabilityType.Embeddings);

        chatA.Priority.ShouldBe(0);
        workspace.ModelConfigurations
            .Count(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .ShouldBe(1);

        embed.Priority.ShouldBe(0);
        chatB.CapabilityType.ShouldBe(AICapabilityType.Embeddings);
        chatB.Priority.ShouldBe(1);
    }

    [Fact]
    public void Seeded_tie_gap_and_negative_heal_on_next_mutate()
    {
        var workspace = CreateWorkspace();
        var early = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "early");
        var mid = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "mid");
        var late = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "late");

        ObjectHelper.TrySetProperty(early, item => item.CreationTime, () => new DateTime(2020, 1, 1));
        ObjectHelper.TrySetProperty(mid, item => item.CreationTime, () => new DateTime(2020, 1, 2));
        ObjectHelper.TrySetProperty(late, item => item.CreationTime, () => new DateTime(2020, 1, 3));

        early.SetPriority(5);
        mid.SetPriority(5);
        late.SetPriority(-3);

        workspace.MoveModelConfiguration(late.Id, position: 1);

        var ordered = workspace.ModelConfigurations
            .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .OrderBy(route => route.Priority)
            .ToList();
        ordered.Select(route => route.Priority).ShouldBe(new[] { 0, 1, 2 });
        ordered.ShouldAllBe(route => route.Priority >= 0);
    }

    [Fact]
    public void Removing_the_only_enabled_default_is_rejected()
    {
        var workspace = CreateWorkspace();
        var only = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "only");

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            workspace.RemoveModelConfiguration(only.Id));
        exception.Code.ShouldBe(AIErrorCodes.CannotDeleteWorkspaceDefault);

        exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            workspace.DisableModelConfiguration(only.Id));
        exception.Code.ShouldBe(AIErrorCodes.CannotDeleteWorkspaceDefault);
    }

    [Fact]
    public void Disabled_routes_share_the_capability_sequence()
    {
        var workspace = CreateWorkspace();
        var first = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "first");
        var second = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "second");
        workspace.DisableModelConfiguration(second.Id);

        first.Priority.ShouldBe(0);
        second.Priority.ShouldBe(1);
        second.IsEnabled.ShouldBeFalse();
        workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.Id.ShouldBe(first.Id);
    }

    [Fact]
    public void NormalizeRoutePriorities_heals_ties_gaps_and_keeps_enabled_winner()
    {
        var workspace = CreateWorkspace();
        var early = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "early");
        var mid = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "mid");
        var late = workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, "late");

        ObjectHelper.TrySetProperty(early, item => item.CreationTime, () => new DateTime(2020, 1, 1));
        ObjectHelper.TrySetProperty(mid, item => item.CreationTime, () => new DateTime(2020, 1, 2));
        ObjectHelper.TrySetProperty(late, item => item.CreationTime, () => new DateTime(2020, 1, 3));

        early.SetPriority(5);
        mid.SetPriority(5);
        late.SetPriority(-3);

        var winnerBefore = workspace.GetPrimaryConfiguration(AICapabilityType.ChatCompletion)!.Id;
        winnerBefore.ShouldBe(late.Id);

        workspace.NormalizeRoutePriorities().ShouldBeTrue();

        var ordered = workspace.ModelConfigurations
            .Where(route => route.CapabilityType == AICapabilityType.ChatCompletion)
            .OrderBy(route => route.Priority)
            .ToList();
        ordered.Select(route => route.Priority).ShouldBe(new[] { 0, 1, 2 });
        ordered[0].Id.ShouldBe(late.Id);
        workspace.NormalizeRoutePriorities().ShouldBeFalse();
    }

    private static Workspace CreateWorkspace() =>
        new(Guid.NewGuid(), "route-order", AIProviderType.OpenAI, "fallback-model");
}
