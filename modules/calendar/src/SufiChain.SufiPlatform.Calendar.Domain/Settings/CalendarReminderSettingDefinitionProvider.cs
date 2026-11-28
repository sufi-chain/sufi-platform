using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Calendar.Reminders;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Calendar.Settings;

public class CalendarReminderSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                CalendarReminderSettings.DispatchIntervalSeconds,
                CalendarReminderSettings.DefaultDispatchIntervalSeconds.ToString(),
                displayName: L("Setting:Calendar.Reminders.DispatchIntervalSeconds"),
                isVisibleToClients: false,
                isInherited: true),
            new SettingDefinition(
                CalendarReminderSettings.CatchUpMinutes,
                CalendarReminderSettings.DefaultCatchUpMinutes.ToString(),
                displayName: L("Setting:Calendar.Reminders.CatchUpMinutes"),
                isVisibleToClients: false,
                isInherited: true));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<CalendarResource>(name);
}
