using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public class DefaultWorkspaceManagedFieldReconcilerTests
{
    [Fact]
    public void Apply_Should_Add_Missing_Capabilities_Without_Overwriting_Administrator_Fields()
    {
        var workspace = new Workspace(Guid.NewGuid(), "Default", AIProviderType.OpenAI, "gpt-4o-mini");
        workspace.UpdateConfiguration("gpt-4o-mini", "sk-admin-owned", "https://admin.example/v1");
        var seed = new DefaultWorkspaceSeedOptions
        {
            Model = "replacement-model",
            EmbeddingModel = "text-embedding-3-small",
            ApiKey = "sk-seed-must-not-win",
            ApiBaseUrl = "https://seed.example/v1"
        };

        var preview = DefaultWorkspaceManagedFieldReconciler.Preview(workspace, seed);
        preview.MissingCapabilities.ShouldContain(AICapabilityType.ChatCompletion);
        preview.MissingCapabilities.ShouldContain(AICapabilityType.Embeddings);
        preview.MissingCapabilities.ShouldNotContain(AICapabilityType.AudioTranscription);

        DefaultWorkspaceManagedFieldReconciler.Apply(workspace, seed, preview);

        workspace.ApiKey.ShouldBe("sk-admin-owned");
        workspace.ApiBaseUrl.ShouldBe("https://admin.example/v1");
        workspace.DefaultModel.ShouldBe("gpt-4o-mini");
        workspace.Model.ShouldBe("gpt-4o-mini");
        workspace.GetPrimaryConfiguration(AICapabilityType.Embeddings)!.ModelId.ShouldBe("text-embedding-3-small");
        workspace.GetPrimaryConfiguration(AICapabilityType.Embeddings)!.ApiKey.ShouldBeNull();
        DefaultWorkspaceManagedFieldReconciler.Preview(workspace, seed).HasChanges.ShouldBeFalse();
    }

    [Fact]
    public void Preview_Should_Reject_Empty_Managed_Models()
    {
        var workspace = new Workspace(Guid.NewGuid(), "Default", AIProviderType.OpenAI, "gpt-4o-mini");
        workspace.AddModelConfiguration(AICapabilityType.ChatCompletion, workspace.DefaultModel);
        var seed = new DefaultWorkspaceSeedOptions
        {
            EmbeddingModel = " ",
            AudioModel = ""
        };

        var preview = DefaultWorkspaceManagedFieldReconciler.Preview(workspace, seed);

        preview.MissingCapabilities.ShouldBeEmpty();
        preview.SeedVersionMissing.ShouldBeTrue();
    }
}
