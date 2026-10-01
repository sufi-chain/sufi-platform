using SufiChain.SufiPlatform.AuditLogging.Hooshvare;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.AuditLogging.Data;

public class AuditInvestigatorHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public AuditInvestigatorHooshvareDataSeedContributor(
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
        var hooshvareKey = AuditInvestigatorHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            AuditInvestigatorHooshvareKeys.Shortcuts.RecentFailures,
            AuditInvestigatorHooshvareKeys.Shortcuts.ExplainSelection
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = AuditInvestigatorHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "AuditLogging",
                    LocalizationResourceName = AuditInvestigatorHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "Investigator",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = AuditInvestigatorHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = true,
                        AllowedMcpToolNames =
                        [
                            AuditInvestigatorHooshvareKeys.Tools.SearchLogs,
                            AuditInvestigatorHooshvareKeys.Tools.GetLog,
                            AuditInvestigatorHooshvareKeys.Tools.SearchEntityChanges,
                            AuditInvestigatorHooshvareKeys.Tools.GetEntityChange
                        ]
                    },
                    ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                        AuditInvestigatorHooshvareKeys.LocalizationResourceName,
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
