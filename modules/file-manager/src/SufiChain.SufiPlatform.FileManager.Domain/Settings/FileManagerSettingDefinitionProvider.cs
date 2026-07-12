using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Settings;

public class FileManagerSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                FileManagerSettings.StorageQuota,
                "1024", // Default 1GB in MB
                isVisibleToClients: true,
                isInherited: true,
                isEncrypted: false
            ),
            
            new SettingDefinition(
                FileManagerSettings.MaxFileSize,
                "104857600", // Default 100MB in bytes
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.AllowedImageExtensions,
                "jpg,jpeg,png,gif,webp,svg",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.AllowedVideoExtensions,
                "mp4,webm,ogg,mov,avi",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.AllowedDocumentExtensions,
                "pdf,doc,docx,xls,xlsx,ppt,pptx,txt",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.EnableWebPConversion,
                "true",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.WebPQuality,
                "80",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.ThumbnailWidth,
                "200",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.ThumbnailHeight,
                "200",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.MaxImageWidth,
                "4096",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.MaxImageHeight,
                "4096",
                isVisibleToClients: true,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.AutoDeleteTempMediaAfterDays,
                "7", // Delete temp media after 7 days
                isVisibleToClients: false,
                isInherited: true
            ),
            
            new SettingDefinition(
                FileManagerSettings.EnableDuplicateDetection,
                "true",
                isVisibleToClients: true,
                isInherited: true
            )
        );
    }
}
