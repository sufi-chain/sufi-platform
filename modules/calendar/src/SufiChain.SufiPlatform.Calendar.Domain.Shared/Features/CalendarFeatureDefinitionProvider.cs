using SufiChain.SufiPlatform.Calendar.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiPlatform.Calendar.Features;

public class CalendarFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(CalendarFeatures.GroupName, L("Menu:SufiCalendar"));
        group.AddFeature(
            CalendarFeatures.Assistant,
            defaultValue: "true",
            displayName: L("Feature:SufiCalendar.Assistant"),
            description: L("Feature:SufiCalendar.Assistant.Description"),
            valueType: new ToggleStringValueType());
    }

    private static LocalizableString L(string name) => LocalizableString.Create<CalendarResource>(name);
}
