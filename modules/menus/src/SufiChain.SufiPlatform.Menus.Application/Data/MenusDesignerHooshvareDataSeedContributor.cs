using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Menus.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Menus.Data;

public class MenusDesignerHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public MenusDesignerHooshvareDataSeedContributor(
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
        var hooshvareKey = MenusDesignerHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            MenusDesignerHooshvareKeys.Shortcuts.DraftItem,
            MenusDesignerHooshvareKeys.Shortcuts.ReviewTree
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = MenusDesignerHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "Menus",
                    RequiredFeatureName = SufiMenusFeatures.Enable,
                    LocalizationResourceName = MenusDesignerHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "Designer",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = MenusDesignerHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = false
                    },
                    ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                        MenusDesignerHooshvareKeys.LocalizationResourceName,
                        shortcutKeys,
                        defaultCulture: "fa"),
                    ShortcutCapabilities = shortcutKeys.ToDictionary(key => key, _ => HooshvareShortcutCapability.Context, StringComparer.Ordinal),
                    RequiredContextKeys = [MenusDesignerHooshvareKeys.Context.MenuId]
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
