using SufiChain.SufiPlatform.Calendar.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Data;

public class CalendarAssistantHooshvareDataSeedContributor : IDataSeedContributor, ITransientDependency, IPlatformHooshvareSeedSource
{
    protected IPlatformHooshvareDefinitionSeeder Seeder { get; }
    protected HooshvareLocalizedShortcutPromptsBuilder ShortcutPromptsBuilder { get; }
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public CalendarAssistantHooshvareDataSeedContributor(
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
        var hooshvareKey = CalendarAssistantHooshvareKeys.Key;
        var shortcutKeys = CalendarAssistantHooshvareKeys.ShortcutIds
            .Select(id => BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, id))
            .ToList();

        return
        [
            new PlatformHooshvareSeedSnapshot
            {
                Key = hooshvareKey,
                Texts = CalendarAssistantHooshvareSeedTexts.Texts,
                Definition = new PlatformHooshvareSeedDefinition
        {
            Key = hooshvareKey,
            SourceModule = "Calendar",
            LocalizationResourceName = CalendarAssistantHooshvareKeys.LocalizationResourceName,
            DisplayName = BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey),
            Kind = HooshvareKind.Copilot,
            Purpose = "Assistant",
            DefaultEnabled = true,
            PersistChatSession = true,
            IsPublic = true,
            HasPublicSafetyPolicy = true,
            EntityVersion = CalendarAssistantHooshvareKeys.EntityVersion,
            SystemPrompt = BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey),
            RuntimeOptions = new HooshvareRuntimeOptions
            {
                UseMcpTools = true,
                AllowedMcpToolNames =
                [
                    CalendarAIToolNames.ListCalendars,
                    CalendarAIToolNames.GetCurrentTime,
                    CalendarAIToolNames.GetWorkingHours,
                    CalendarAIToolNames.GetFreeBusy,
                    CalendarAIToolNames.FindFreeSlots,
                    CalendarAIToolNames.SearchEvents,
                    CalendarAIToolNames.CreateEvent,
                    CalendarAIToolNames.MoveEvent,
                    CalendarAIToolNames.MoveOccurrence,
                    CalendarAIToolNames.CancelEvent,
                    CalendarAIToolNames.CancelOccurrence,
                    CalendarAIToolNames.TestAvailability
                ]
            },
            ShortcutPromptsJson = ShortcutPromptsBuilder.BuildKeysJson(
                CalendarAssistantHooshvareKeys.LocalizationResourceName,
                shortcutKeys,
                defaultCulture: "fa"),
            ShortcutCapabilities = shortcutKeys.ToDictionary(
                key => key,
                _ => HooshvareShortcutCapability.Mcp,
                StringComparer.Ordinal),
            RequiredContextKeys = ["calendarId"]
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
