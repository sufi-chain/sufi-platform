using SufiChain.SufiAbp.SettingManagement.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace SufiChain.SufiAbp.SettingManagement;

public class SettingManagementFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            SettingManagementFeatures.GroupName,
            L("Feature:SettingManagementGroup"));

        var settingEnableFeature = group.AddFeature(
            SettingManagementFeatures.Enable,
            "true",
            L("Feature:SettingManagementEnable"),
            L("Feature:SettingManagementEnableDescription"),
            new ToggleStringValueType(),
            isAvailableToHost: false);

        settingEnableFeature.CreateChild(
            SettingManagementFeatures.AllowChangingEmailSettings,
            "false",
            L("Feature:AllowChangingEmailSettings"),
            L("Feature:AllowChangingEmailSettingsDescription"),
            new ToggleStringValueType(),
            isAvailableToHost: false);
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SufiAbpSettingManagementResource>(name);
    }
}
