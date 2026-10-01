using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Tags.Features;
using SufiChain.SufiPlatform.Tags.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Tags.Data;

public class TagsTaxonomyHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public TagsTaxonomyHooshvareDataSeedContributor(
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
        var hooshvareKey = TagsTaxonomyHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            TagsTaxonomyHooshvareKeys.Shortcuts.SearchScope,
            TagsTaxonomyHooshvareKeys.Shortcuts.ExplainSelection
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = TagsTaxonomyHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "Tags",
                    RequiredFeatureName = SufiTagsFeatures.Enable,
                    LocalizationResourceName = TagsTaxonomyHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "Taxonomy",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = TagsTaxonomyHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = true,
                        AllowedMcpToolNames =
                        [
                            TagsTaxonomyHooshvareKeys.Tools.Search,
                            TagsTaxonomyHooshvareKeys.Tools.Get,
                            TagsTaxonomyHooshvareKeys.Tools.ListByScope,
                            TagsTaxonomyHooshvareKeys.Tools.GetLinksByTag,
                            TagsTaxonomyHooshvareKeys.Tools.GetTagsByEntity
                        ]
                    },
                    ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                        TagsTaxonomyHooshvareKeys.LocalizationResourceName,
                        shortcutKeys,
                        defaultCulture: "fa"),
                    ShortcutCapabilities = shortcutKeys.ToDictionary(key => key, _ => HooshvareShortcutCapability.Mcp, StringComparer.Ordinal)
                }
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
}
