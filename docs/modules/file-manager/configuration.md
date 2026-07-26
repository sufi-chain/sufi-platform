# File Manager Configuration

The File Manager module has a meaningful configuration surface across storage, processing, validation, and quotas.

## Key settings defined in source

| Setting | Default | Purpose |
| --- | --- | --- |
| `SufiFileManager.StorageQuota` | `1024` | Per-tenant storage quota in MB |
| `SufiFileManager.MaxFileSize` | `104857600` | Maximum file size in bytes |
| `SufiFileManager.AllowedImageExtensions` | `jpg,jpeg,png,gif,webp,svg` | Allowed image file extensions |
| `SufiFileManager.AllowedVideoExtensions` | `mp4,webm,ogg,mov,avi` | Allowed video file extensions |
| `SufiFileManager.AllowedDocumentExtensions` | `pdf,doc,docx,xls,xlsx,ppt,pptx,txt` | Allowed document file extensions |
| `SufiFileManager.EnableWebPConversion` | `true` | Enables WebP conversion |
| `SufiFileManager.WebPQuality` | `80` | WebP quality level |
| `SufiFileManager.ThumbnailWidth` | `200` | Default thumbnail width |
| `SufiFileManager.ThumbnailHeight` | `200` | Default thumbnail height |
| `SufiFileManager.MaxImageWidth` | `4096` | Maximum image width |
| `SufiFileManager.MaxImageHeight` | `4096` | Maximum image height |
| `SufiFileManager.AutoDeleteTempMediaAfterDays` | `7` | Temporary-media cleanup retention |
| `SufiFileManager.EnableDuplicateDetection` | `true` | Duplicate detection toggle |

## Additional configuration areas

- file structures and validation rules
- blob storage provider selection
- public versus private asset access behavior
- rich text integration behavior
- server/WASM access-token and URL resolution behavior

## Related guidance

- [Integration Guide](integration-guide.md)
- [Configuration details](configuration.md)
- [Image Editor Guide](image-editor-guide.md)
