using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Hooshvare
{
    /// <summary>
    /// Platform hooshvare key for the Calendar assistant.
    /// </summary>
    public static class CalendarAssistantHooshvareKeys
    {
        public const string Key = PlatformHooshvareKeys.CalendarAssistant;

        public const string LocalizationResourceName = PlatformHooshvareKeys.LocalizationResources.Calendar;

        public const int EntityVersion = 1;

        public static readonly string[] ShortcutIds =
        [
            "DayAgenda",
            "FreeSlot",
            "ScheduleTomorrow",
            "FreeFriday"
        ];
    }
}
