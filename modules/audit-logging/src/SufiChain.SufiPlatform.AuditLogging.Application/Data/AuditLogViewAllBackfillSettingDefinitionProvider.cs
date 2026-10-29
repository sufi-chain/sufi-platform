using SufiChain.SufiPlatform.AuditLogging;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.AuditLogging.Data;

public class AuditLogViewAllBackfillSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(new SettingDefinition(
            AuditLogViewAllBackfill.CompletedSettingName,
            defaultValue: "false",
            isVisibleToClients: false));
    }
}
