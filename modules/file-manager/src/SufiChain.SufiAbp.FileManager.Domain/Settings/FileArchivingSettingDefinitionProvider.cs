using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.FileManager.Settings;

/// <summary>
/// Setting definition provider for file archiving
/// </summary>
public class FileArchivingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                FileArchivingSettings.Enabled,
                "true",
                isVisibleToClients: true,
                isEncrypted: false),
            
            new SettingDefinition(
                FileArchivingSettings.RetentionDays,
                "90",
                isVisibleToClients: true,
                isEncrypted: false),
            
            new SettingDefinition(
                FileArchivingSettings.BatchSize,
                "100",
                isVisibleToClients: false,
                isEncrypted: false),
            
            new SettingDefinition(
                FileArchivingSettings.Schedule,
                "0 2 * * *",
                isVisibleToClients: false,
                isEncrypted: false),
            
            new SettingDefinition(
                FileArchivingSettings.ArchiveAIFiles,
                "true",
                isVisibleToClients: true,
                isEncrypted: false),
            
            new SettingDefinition(
                FileArchivingSettings.AIFilesRetentionDays,
                null,
                isVisibleToClients: true,
                isEncrypted: false)
        );
    }
}
