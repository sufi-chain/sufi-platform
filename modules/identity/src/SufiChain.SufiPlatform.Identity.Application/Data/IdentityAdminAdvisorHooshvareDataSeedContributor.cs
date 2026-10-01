using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Identity.Hooshvare;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Identity.Data;

public class IdentityAdminAdvisorHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public IdentityAdminAdvisorHooshvareDataSeedContributor(
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
        var hooshvareKey = IdentityAdminAdvisorHooshvareKeys.Key;
        var shortcutKeys = new[]
        {
            IdentityAdminAdvisorHooshvareKeys.Shortcuts.ExplainUser,
            IdentityAdminAdvisorHooshvareKeys.Shortcuts.RecentSecurity
        }.Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id)).ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = IdentityAdminAdvisorHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
                {
                    Key = hooshvareKey,
                    SourceModule = "Identity",
                    LocalizationResourceName = IdentityAdminAdvisorHooshvareKeys.LocalizationResourceName,
                    DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
                    Kind = HooshvareKind.Assistant,
                    Purpose = "AdminAdvisor",
                    DefaultEnabled = true,
                    PersistChatSession = false,
                    IsPublic = false,
                    EntityVersion = IdentityAdminAdvisorHooshvareKeys.EntityVersion,
                    SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
                    RuntimeOptions = new HooshvareRuntimeOptions
                    {
                        UseRag = false,
                        UseMcpTools = true,
                        AllowedMcpToolNames =
                        [
                            IdentityAdminAdvisorHooshvareKeys.Tools.SearchUsers,
                            IdentityAdminAdvisorHooshvareKeys.Tools.GetUser,
                            IdentityAdminAdvisorHooshvareKeys.Tools.GetUserRoles,
                            IdentityAdminAdvisorHooshvareKeys.Tools.ListRoles,
                            IdentityAdminAdvisorHooshvareKeys.Tools.GetOuTree,
                            IdentityAdminAdvisorHooshvareKeys.Tools.SearchSecurityLogs
                        ]
                    },
                    ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                        IdentityAdminAdvisorHooshvareKeys.LocalizationResourceName,
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
