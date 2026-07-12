using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.Features;

using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Settings;

public class SettingsFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            SettingsFeatures.GroupName,
            L("Feature:SettingsGroup"));

        var settingEnableFeature = group.AddFeature(
            SettingsFeatures.Enable,
            "true",
            L("Feature:SettingsEnable"),
            L("Feature:SettingsEnableDescription"),
            new ToggleStringValueType(),
            isAvailableToHost: false);

        settingEnableFeature.CreateChild(
            SettingsFeatures.AllowChangingEmailSettings,
            "false",
            L("Feature:AllowChangingEmailSettings"),
            L("Feature:AllowChangingEmailSettingsDescription"),
            new ToggleStringValueType(),
            isAvailableToHost: false);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiSettingsResource>(name);
    }
}
