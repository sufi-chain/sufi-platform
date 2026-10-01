using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Preview and apply for managed default-workspace capability rows.
/// Administrator-owned credentials, endpoint, provider, activation, and model selection are left unchanged.
/// </summary>
public static class DefaultWorkspaceManagedFieldReconciler
{
    public const string SeedVersionProperty = "DefaultWorkspaceSeedVersion";

    public const int CurrentSeedVersion = 1;

    public static DefaultWorkspaceManagedFieldPreview Preview(Workspace workspace, DefaultWorkspaceSeedOptions seed)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(seed);

        var missing = new List<AICapabilityType>();
        foreach (var (capability, modelId) in ManagedCapabilities(workspace, seed))
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                continue;
            }

            if (workspace.GetPrimaryConfiguration(capability) == null)
            {
                missing.Add(capability);
            }
        }

        workspace.ExtraProperties.TryGetValue(SeedVersionProperty, out var stored);
        var storedVersion = stored?.ToString();
        var seedVersionMissing = storedVersion != CurrentSeedVersion.ToString();
        return new DefaultWorkspaceManagedFieldPreview(missing, seedVersionMissing);
    }

    public static void Apply(Workspace workspace, DefaultWorkspaceSeedOptions seed, DefaultWorkspaceManagedFieldPreview preview)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(preview);

        var models = ManagedCapabilities(workspace, seed).ToDictionary(item => item.Capability, item => item.ModelId);
        foreach (var capability in preview.MissingCapabilities)
        {
            if (!models.TryGetValue(capability, out var modelId) || string.IsNullOrWhiteSpace(modelId))
            {
                continue;
            }

            if (workspace.GetPrimaryConfiguration(capability) != null)
            {
                continue;
            }

            var dimensions = capability == AICapabilityType.Embeddings
                ? (int?)EmbeddingModelDefaults.GetDimensions(modelId)
                : null;
            workspace.AddModelConfiguration(
                capability,
                modelId.Trim(),
                apiEndpoint: null,
                apiKey: null,
                priority: 0,
                openAIApiMode: OpenAIApiMode.ChatCompletions,
                dimensions: dimensions,
                maxContextTokens: AIModelConfiguration.DefaultMaxContextTokens);
        }

        if (preview.SeedVersionMissing)
        {
            workspace.SetProperty(SeedVersionProperty, CurrentSeedVersion);
        }
    }

    private static IEnumerable<(AICapabilityType Capability, string? ModelId)> ManagedCapabilities(
        Workspace workspace,
        DefaultWorkspaceSeedOptions seed)
    {
        // Chat stays on the administrator-selected default model. Other capabilities are managed
        // only when the workspace has no row for them yet.
        yield return (AICapabilityType.ChatCompletion, workspace.DefaultModel);
        yield return (AICapabilityType.Embeddings, seed.EmbeddingModel);
        yield return (AICapabilityType.AudioTranscription, seed.AudioModel);
        yield return (AICapabilityType.TextToSpeech, seed.TtsModel);
        yield return (AICapabilityType.VisionAnalysis, seed.VisionModel);
        yield return (AICapabilityType.ImageGeneration, seed.ImageModel);
    }
}

public sealed record DefaultWorkspaceManagedFieldPreview(
    IReadOnlyList<AICapabilityType> MissingCapabilities,
    bool SeedVersionMissing)
{
    public bool HasChanges => MissingCapabilities.Count > 0 || SeedVersionMissing;
}
