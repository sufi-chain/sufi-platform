using Volo.Abp.Settings;

namespace MyCompanyName.MyProjectName.Settings
{
    public class DemoAppSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(DemoAppSettings.MySetting1));
        }
    }
}
