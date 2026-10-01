using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Tenants.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Tenants.Data;

public class TenantsDiagnosticsHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public TenantsDiagnosticsHooshvareDataSeedContributor(
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
        var hooshvareKey = TenantsDiagnosticsHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            TenantsDiagnosticsHooshvareKeys.Shortcuts.ListTenants,
            TenantsDiagnosticsHooshvareKeys.Shortcuts.SummarizeSelection
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = TenantsDiagnosticsHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "Tenants",
                    LocalizationResourceName = TenantsDiagnosticsHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "Diagnostics",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = TenantsDiagnosticsHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = true,
                        AllowedMcpToolNames =
                        [
                            TenantsDiagnosticsHooshvareKeys.Tools.List,
                            TenantsDiagnosticsHooshvareKeys.Tools.Get,
                            TenantsDiagnosticsHooshvareKeys.Tools.GetSummary
                        ]
                    },
                    ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                        TenantsDiagnosticsHooshvareKeys.LocalizationResourceName,
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
