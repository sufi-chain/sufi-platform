# File Manager Configuration

The File Manager module has a meaningful configuration surface across storage, processing, validation, and quotas.

## Key settings defined in source

| Setting | Default | Purpose |
| --- | --- | --- |
| `SufiAbpFileManager.StorageQuota` | `1024` | Per-tenant storage quota in MB |
| `SufiAbpFileManager.MaxFileSize` | `104857600` | Maximum file size in bytes |
| `SufiAbpFileManager.AllowedImageExtensions` | `jpg,jpeg,png,gif,webp,svg` | Allowed image file extensions |
| `SufiAbpFileManager.AllowedVideoExtensions` | `mp4,webm,ogg,mov,avi` | Allowed video file extensions |
| `SufiAbpFileManager.AllowedDocumentExtensions` | `pdf,doc,docx,xls,xlsx,ppt,pptx,txt` | Allowed document file extensions |
| `SufiAbpFileManager.EnableWebPConversion` | `true` | Enables WebP conversion |
| `SufiAbpFileManager.WebPQuality` | `80` | WebP quality level |
| `SufiAbpFileManager.ThumbnailWidth` | `200` | Default thumbnail width |
| `SufiAbpFileManager.ThumbnailHeight` | `200` | Default thumbnail height |
| `SufiAbpFileManager.MaxImageWidth` | `4096` | Maximum image width |
| `SufiAbpFileManager.MaxImageHeight` | `4096` | Maximum image height |
| `SufiAbpFileManager.AutoDeleteTempMediaAfterDays` | `7` | Temporary-media cleanup retention |
| `SufiAbpFileManager.EnableDuplicateDetection` | `true` | Duplicate detection toggle |

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
