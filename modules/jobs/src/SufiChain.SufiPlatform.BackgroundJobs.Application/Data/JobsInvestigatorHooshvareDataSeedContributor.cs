using SufiChain.SufiPlatform.BackgroundJobs.Hooshvare;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.BackgroundJobs.Data;

public class JobsInvestigatorHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public JobsInvestigatorHooshvareDataSeedContributor(
        IPlatformHooshvareDefinitionSeeder seeder,
        HooshvareLocalizedShortcutPromptsBuilder shortcutPromptsBuilder,
        ILocalizationTextSeeder localizationTextSeeder)
    {
        Seeder = seeder;
        ShortcutPromptsBuilder = shortcutPromptsBuilder;
        LocalizationTextSeeder = localizationTextSeeder;
    }

    public virtual IReadOnlyList<PlatformHooshvareSeedSnapshot> GetSnapshots()
    {
        var hooshvareKey = JobsInvestigatorHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            JobsInvestigatorHooshvareKeys.Shortcuts.AbandonedJobs,
            JobsInvestigatorHooshvareKeys.Shortcuts.ExplainSelection
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = JobsInvestigatorHooshvareSeedTexts.Texts,
                Definition = ReadOnlyDefinition(
                    hooshvareKey,
                    "BackgroundJobs",
                    "Investigator",
                    JobsInvestigatorHooshvareKeys.LocalizationResourceName,
                    JobsInvestigatorHooshvareKeys.EntityVersion,
                    shortcutKeys,
                    ShortcutPromptsBuilder,
                    [
                        JobsInvestigatorHooshvareKeys.Tools.Search,
                        JobsInvestigatorHooshvareKeys.Tools.Get
                    ])
            }
        ];
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        foreach (var snapshot in GetSnapshots())
        {
            await PlatformHooshvareLocalizationSeeder.SeedAsync(
                LocalizationTextSeeder,
                snapshot.Definition.LocalizationResourceName!,
                snapshot.Key,
                snapshot.Texts!,
                context.TenantId);
            await Seeder.SeedAsync(snapshot.Definition, context);
        }
    }

    internal static PlatformHooshvareSeedDefinition ReadOnlyDefinition(
        string hooshvareKey,
        string sourceModule,
        string purpose,
        string resourceName,
        int entityVersion,
        List<string> shortcutKeys,
        HooshvareLocalizedShortcutPromptsBuilder shortcutPromptsBuilder,
        string[] tools,
        string? requiredFeatureName = null,
        List<string>? requiredContextKeys = null) =>
        new()
        {
            Key = hooshvareKey,
            SourceModule = sourceModule,
            RequiredFeatureName = requiredFeatureName,
            LocalizationResourceName = resourceName,
            DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
            Kind = HooshvareKind.Assistant,
            Purpose = purpose,
            DefaultEnabled = true,
            PersistChatSession = false,
            IsPublic = false,
            EntityVersion = entityVersion,
            SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
            RuntimeOptions = new HooshvareRuntimeOptions
            {
                UseRag = false,
                UseMcpTools = tools.Length > 0,
                AllowedMcpToolNames = tools.ToList()
            },
            ShortcutPromptsJson = shortcutPromptsBuilder.BuildKeysJson(resourceName, shortcutKeys, defaultCulture: "fa"),
            ShortcutCapabilities = shortcutKeys.ToDictionary(
                key => key,
                _ => tools.Length > 0 ? HooshvareShortcutCapability.Mcp : HooshvareShortcutCapability.Context,
                StringComparer.Ordinal),
            RequiredContextKeys = requiredContextKeys ?? []
        };
}
