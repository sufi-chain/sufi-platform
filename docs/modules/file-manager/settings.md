# File Manager Settings

The File Manager module defines a rich settings surface for upload limits, storage quotas, processing, and duplicate detection.

## Defined settings

| Setting | Default | Scope note |
| --- | --- | --- |
| `SufiFileManager.StorageQuota` | `1024` | Inherited setting, client-visible |
| `SufiFileManager.MaxFileSize` | `104857600` | Inherited setting, client-visible |
| `SufiFileManager.AllowedImageExtensions` | `jpg,jpeg,png,gif,webp,svg` | Inherited setting, client-visible |
| `SufiFileManager.AllowedVideoExtensions` | `mp4,webm,ogg,mov,avi` | Inherited setting, client-visible |
| `SufiFileManager.AllowedDocumentExtensions` | `pdf,doc,docx,xls,xlsx,ppt,pptx,txt` | Inherited setting, client-visible |
| `SufiFileManager.EnableWebPConversion` | `true` | Inherited setting, client-visible |
| `SufiFileManager.WebPQuality` | `80` | Inherited setting, client-visible |
| `SufiFileManager.ThumbnailWidth` | `200` | Inherited setting, client-visible |
| `SufiFileManager.ThumbnailHeight` | `200` | Inherited setting, client-visible |
| `SufiFileManager.MaxImageWidth` | `4096` | Inherited setting, client-visible |
| `SufiFileManager.MaxImageHeight` | `4096` | Inherited setting, client-visible |
| `SufiFileManager.AutoDeleteTempMediaAfterDays` | `7` | Inherited setting, not client-visible |
| `SufiFileManager.EnableDuplicateDetection` | `true` | Inherited setting, client-visible |

## UI integration

The Blazor package includes a storage settings group contributor, which indicates these settings can be surfaced through the platform settings experience.
