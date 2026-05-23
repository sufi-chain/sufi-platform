# File Manager Settings

The File Manager module defines a rich settings surface for upload limits, storage quotas, processing, and duplicate detection.

## Defined settings

| Setting | Default | Scope note |
| --- | --- | --- |
| `SufiAbpFileManager.StorageQuota` | `1024` | Inherited setting, client-visible |
| `SufiAbpFileManager.MaxFileSize` | `104857600` | Inherited setting, client-visible |
| `SufiAbpFileManager.AllowedImageExtensions` | `jpg,jpeg,png,gif,webp,svg` | Inherited setting, client-visible |
| `SufiAbpFileManager.AllowedVideoExtensions` | `mp4,webm,ogg,mov,avi` | Inherited setting, client-visible |
| `SufiAbpFileManager.AllowedDocumentExtensions` | `pdf,doc,docx,xls,xlsx,ppt,pptx,txt` | Inherited setting, client-visible |
| `SufiAbpFileManager.EnableWebPConversion` | `true` | Inherited setting, client-visible |
| `SufiAbpFileManager.WebPQuality` | `80` | Inherited setting, client-visible |
| `SufiAbpFileManager.ThumbnailWidth` | `200` | Inherited setting, client-visible |
| `SufiAbpFileManager.ThumbnailHeight` | `200` | Inherited setting, client-visible |
| `SufiAbpFileManager.MaxImageWidth` | `4096` | Inherited setting, client-visible |
| `SufiAbpFileManager.MaxImageHeight` | `4096` | Inherited setting, client-visible |
| `SufiAbpFileManager.AutoDeleteTempMediaAfterDays` | `7` | Inherited setting, not client-visible |
| `SufiAbpFileManager.EnableDuplicateDetection` | `true` | Inherited setting, client-visible |

## UI integration

The Blazor package includes a storage settings group contributor, which indicates these settings can be surfaced through the platform settings experience.
