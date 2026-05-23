using Volo.Abp.Emailing;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Overrides ABP email setting defaults with Sufi Platform values (sufichain.ir domain).
/// </summary>
public class SufiAbpConstantsSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var defaultFromAddress = context.GetOrNull(EmailSettingNames.DefaultFromAddress);
        if (defaultFromAddress != null)
        {
            defaultFromAddress.DefaultValue = SufiAbpConstants.DefaultFromAddress;
        }

        var defaultFromDisplayName = context.GetOrNull(EmailSettingNames.DefaultFromDisplayName);
        if (defaultFromDisplayName != null)
        {
            defaultFromDisplayName.DefaultValue = SufiAbpConstants.DefaultFromDisplayName;
        }
    }
}
