# SufiChain.SufiPlatform.FileManager – API Reference

## Application Services

### IFileItemAppService

Main service for file item operations.

**Namespace:** `SufiChain.SufiPlatform.FileManager.FileItems`

#### UploadAsync

Upload a single file (small files; loads into memory).

```csharp
Task<FileItemDto> UploadAsync(UploadFileInput input)
```

**Parameters:**
- `FileName` (string) – Original file name
- `Content` (byte[]) – File content
- `MimeType` (string) – MIME type
- `StructureKey` (string?) – File structure key for validation
- `EntityType` (string?) – Associated entity type
- `EntityId` (Guid?) – Associated entity ID
- `AutoConfirm` (bool) – Auto-confirm or upload as temp
- `Alt` (string?) – Alt text for images. Public access is determined by the structure's **IsPublicAccess**, not per-file.
- `Alt` (string?) – Alt text for images

**Returns:** `FileItemDto`

---

#### UploadStreamAsync

Upload a file using streaming (memory-efficient for large files).

```csharp
Task<FileItemDto> UploadStreamAsync(UploadFileStreamInput input)
```

**Parameters:**
- `FileName`, `ContentStream`, `ContentLength`, `MimeType`, `StructureKey`, `EntityType`, `EntityId`, `AutoConfirm`, `Alt`, `SkipProcessing`

---

#### UploadMultipleAsync

Upload multiple files.

```csharp
Task<ListResultDto<FileItemDto>> UploadMultipleAsync(UploadMultipleFileInput input)
```

---

#### GetAsync

Get a file item by ID.

```csharp
Task<FileItemDto> GetAsync(Guid id)
```

---

#### GetListAsync

Get a list of file items with filtering and paging.

```csharp
Task<PagedResultDto<FileItemDto>> GetListAsync(GetFileListInput input)
```

**Parameters:**
- `Keyword` (string?) – Search in file names
- `FileType` (FileType?) – Filter by type (Image, Video, Document, Audio)
- `EntityType` (string?) – Filter by entity type
- `EntityId` (Guid?) – Filter by entity ID
- `StructureKey` (string?) – Filter by structure
- `OnlyFromPublicStructures` (bool?) – Filter to files from structures with IsPublicAccess = true
- `IsTemp` (bool?) – Filter by temp status
- `Sorting` (string?) – e.g. `"CreationTime DESC"`
- `SkipCount`, `MaxResultCount` – Pagination

---

#### DeleteAsync / DeleteManyAsync

```csharp
Task DeleteAsync(Guid id)
Task DeleteManyAsync(Guid[] ids)
```

---

#### UpdateMetadataAsync

Update file metadata (alt text, name, tags).

```csharp
Task<FileItemDto> UpdateMetadataAsync(Guid id, UpdateFileMetadataInput input)
```

---

#### GetDownloadUrlAsync / GetThumbnailUrlAsync

```csharp
Task<string> GetDownloadUrlAsync(Guid id)
Task<string> GetThumbnailUrlAsync(Guid id)
```

---

#### GetStatisticsAsync

```csharp
Task<FileStatisticsDto> GetStatisticsAsync()
```

**Returns:**
```csharp
public class FileStatisticsDto
{
    public int TotalCount { get; set; }
    public int ImageCount { get; set; }
    public int VideoCount { get; set; }
    public int DocumentCount { get; set; }
    public int AudioCount { get; set; }
    public int OtherCount { get; set; }
    public long TotalSize { get; set; }
}
```

---

#### GetStorageQuotaAsync

```csharp
Task<StorageQuotaDto> GetStorageQuotaAsync()
```

**Returns:**
```csharp
public class StorageQuotaDto
{
    public long UsedBytes { get; set; }
    public long LimitBytes { get; set; }
    public double PercentageUsed { get; set; }
    public bool IsUnlimited { get; set; }
}
```

---

#### ConfirmAsync

Confirm a temporary file (move from temp to permanent storage).

```csharp
Task<FileItemDto> ConfirmAsync(Guid id)
```

---

### IFileStructureAppService

Service for managing file structures.

**Namespace:** `SufiChain.SufiPlatform.FileManager.FileStructures`

```csharp
Task<FileStructureDto> GetAsync(Guid id)
Task<FileStructureDto> GetByKeyAsync(string key)
Task<ListResultDto<FileStructureDto>> GetListAsync()
Task<FileStructureDto> CreateAsync(CreateUpdateFileStructureInput input)
Task<FileStructureDto> UpdateAsync(Guid id, CreateUpdateFileStructureInput input)
Task DeleteAsync(Guid id)
```

---

### IFolderAppService

Service for managing folders.

**Namespace:** `SufiChain.SufiPlatform.FileManager.FileFolders`

```csharp
Task<FolderDto> CreateAsync(CreateFolderInput input)
Task<FolderDto> UpdateAsync(Guid id, UpdateFolderInput input)
Task DeleteAsync(Guid id)
Task<FolderDto> GetAsync(Guid id)
Task<ListResultDto<FolderTreeNodeDto>> GetTreeAsync(Guid? parentId = null)
Task<FolderContentsDto> GetContentsAsync(Guid? folderId, GetFolderContentsInput input)
```

---

## DTOs

### FileItemDto

```csharp
public class FileItemDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public string BlobName { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public FileType FileType { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? ThumbnailBlobName { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string? Alt { get; set; }
    public List<string>? Tags { get; set; }
    public bool StructureIsPublicAccess { get; set; }
    public string? StructureBaseUrl { get; set; }
    public bool IsTemp { get; set; }
    public string? StructureKey { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
```

### FileStructureDto

```csharp
public class FileStructureDto : EntityDto<Guid>
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public string? Description { get; set; }
    public FileType AllowedFileTypes { get; set; }
    public string? AllowedExtensions { get; set; }
    public string? AllowedMimeTypes { get; set; }
    public long MaxFileSize { get; set; }
    public int? MinImageWidth { get; set; }
    public int? MinImageHeight { get; set; }
    public int? MaxImageWidth { get; set; }
    public int? MaxImageHeight { get; set; }
    public bool IsMultiple { get; set; }
    public int? MaxCount { get; set; }
    public bool IsRequired { get; set; }
    public bool GenerateThumbnail { get; set; }
    public int ThumbnailWidth { get; set; }
    public int ThumbnailHeight { get; set; }
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; }
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }
}
```

---

## Enums

### FileType

```csharp
[Flags]
public enum FileType
{
    None = 0,
    Image = 1,
    Video = 2,
    Document = 4,
    Audio = 8
}
```

---

## HTTP API Endpoints

**Base path:** `api/sabp/file-manager`

### File Items

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/file-items/upload` | Upload single file (multipart) |
| POST | `/file-items/upload-stream` | Upload single file (streaming) |
| POST | `/file-items/upload-multiple` | Upload multiple files |
| GET | `/file-items/{id}` | Get file item |
| GET | `/file-items` | List file items (query params for filter/page) |
| GET | `/file-items/{id}/download` | Download file |
| GET | `/file-items/{id}/stream` | Stream file (range requests) |
| GET | `/file-items/{id}/thumbnail` | Get thumbnail |
| PUT | `/file-items/{id}/metadata` | Update metadata |
| POST | `/file-items/{id}/confirm` | Confirm temp file |
| DELETE | `/file-items/{id}` | Delete file |
| POST | `/file-items/delete-many` | Delete multiple files |
| GET | `/file-items/storage-quota` | Get storage quota |
| GET | `/file-items/statistics` | Get statistics |
| GET | `/file-items/{id}/download-url` | Get download URL |
| GET | `/file-items/{id}/thumbnail-url` | Get thumbnail URL |

### File Structures

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/file-structures/{id}` | Get structure |
| GET | `/file-structures/by-key/{key}` | Get by key |
| GET | `/file-structures` | List structures |
| POST | `/file-structures` | Create structure |
| PUT | `/file-structures/{id}` | Update structure |
| DELETE | `/file-structures/{id}` | Delete structure |

### Folders

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/folders/{id}` | Get folder |
| GET | `/folders/tree` | Get folder tree |
| GET | `/folders/{id}/contents` | Get folder contents |
| POST | `/folders` | Create folder |
| PUT | `/folders/{id}` | Update folder |
| DELETE | `/folders/{id}` | Delete folder |

---

## Permissions

| Permission | Description |
|------------|-------------|
| `SufiAbpFileManager.FileItems` | View file items |
| `SufiAbpFileManager.FileItems.Create` | Upload |
| `SufiAbpFileManager.FileItems.Update` | Update metadata |
| `SufiAbpFileManager.FileItems.Delete` | Delete |
| `SufiAbpFileManager.FileStructures` | View structures |
| `SufiAbpFileManager.FileStructures.Create` | Create |
| `SufiAbpFileManager.FileStructures.Update` | Update |
| `SufiAbpFileManager.FileStructures.Delete` | Delete |

---

## Blazor URL Provider

For tiered apps (Blazor and API on different origins), use **IFileItemUrlProvider** from `SufiChain.SufiPlatform.FileManager.Blazor.Public.Services`:

```csharp
public interface IFileItemUrlProvider
{
    string ApiBaseUrl { get; }
    string GetThumbnailUrl(Guid fileItemId);
    string GetDownloadUrl(Guid fileItemId);
    string GetStreamUrl(Guid fileItemId);
}
```

Reads `RemoteServices:SufiAbpFileManager:BaseUrl` or `RemoteServices:Default:BaseUrl` from configuration.

---

## See Also

- [Configuration](configuration.md)
- [Integration guide](integration-guide.md)
- [Blazor components guide](blazor-components-guide.md)
