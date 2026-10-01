using System.Linq;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public class WorkspaceReadinessHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public WorkspaceReadinessHooshvareDataSeedContributor(
        IPlatformHooshvareDefinitionSeeder seeder,
        ILocalizationTextSeeder localizationTextSeeder)
    {
        Seeder = seeder;
        LocalizationTextSeeder = localizationTextSeeder;
    }

    public virtual IReadOnlyList<PlatformHooshvareSeedSnapshot> GetSnapshots()
    {
        var hooshvareKey = WorkspaceReadinessHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            WorkspaceReadinessHooshvareKeys.Shortcuts.SummarizeReadiness,
            WorkspaceReadinessHooshvareKeys.Shortcuts.CheckRag
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = WorkspaceReadinessHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "SufiAI",
                    RequiredFeatureName = SufiAIFeatures.Enable,
                    LocalizationResourceName = WorkspaceReadinessHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "WorkspaceReadiness",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = WorkspaceReadinessHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = true,
                        AllowedMcpToolNames =
                        [
                            WorkspaceReadinessHooshvareKeys.Tools.ListWorkspaces,
                            WorkspaceReadinessHooshvareKeys.Tools.GetWorkspaceReadiness,
                            WorkspaceReadinessHooshvareKeys.Tools.ListModelCapabilities,
                            WorkspaceReadinessHooshvareKeys.Tools.GetRagAvailability,
                            WorkspaceReadinessHooshvareKeys.Tools.GetMcpCatalogSummary
                        ]
                    },
                    ShortcutPromptsJson = HooshvareShortcutPromptsDocument.Serialize(new HooshvareShortcutPromptsDocument
                    {
                        Version = HooshvareShortcutPromptsDocument.CurrentVersion,
                        ResourceName = WorkspaceReadinessHooshvareKeys.LocalizationResourceName,
                        DefaultCulture = "fa",
                        Keys = shortcutKeys
                    }),
                    ShortcutCapabilities = shortcutKeys.ToDictionary(
                        key => key,
                        _ => HooshvareShortcutCapability.Mcp,
                        StringComparer.Ordinal)
                }
            }
        ];
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        foreach (var snapshot in GetSnapshots())
        {
            var overwriteExisting = context.TenantId == null;
            var texts = snapshot.Texts!;
            await LocalizationTextSeeder.UpsertAsync(
                snapshot.Definition.LocalizationResourceName!,
                BusinessLocalizationKeys.HooshvareDisplayName(snapshot.Key),
                texts.DisplayName,
                context.TenantId,
                overwriteExisting);
            await LocalizationTextSeeder.UpsertAsync(
                snapshot.Definition.LocalizationResourceName!,
                BusinessLocalizationKeys.HooshvareSystemPrompt(snapshot.Key),
                texts.SystemPrompt,
                context.TenantId,
                overwriteExisting: true);
            foreach (var (shortcutId, cultureValues) in texts.Shortcuts)
            {
                await LocalizationTextSeeder.UpsertAsync(
                    snapshot.Definition.LocalizationResourceName!,
                    BusinessLocalizationKeys.HooshvareShortcut(snapshot.Key, shortcutId),
                    cultureValues,
                    context.TenantId,
                    overwriteExisting);
            }

            await Seeder.SeedAsync(snapshot.Definition, context);
        }
    }
}
