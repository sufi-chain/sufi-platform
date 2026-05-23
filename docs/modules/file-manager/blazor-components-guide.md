# Blazor Components Guide

SufiChain.SufiAbp.FileManager provides Blazor components for file upload, browsing, and display in both admin and public-facing UIs.

---

## Component Overview

### Admin (SufiChain.SufiAbp.FileManager.Blazor)

| Component | Description |
|-----------|-------------|
| **SufiAbpFileUploader** | Multi-file uploader with drag-drop, progress, structure selection |
| **SufiAbpQuickImageUploader** | Simple single-image uploader with preview |
| **SufiAbpFileBrowser** | Grid/list browser with search, filters, selection, bulk delete |
| **SufiAbpFileGallery** | Gallery view with pagination, filters, lightbox |
| **SufiAbpFileSelector** | Modal for selecting existing files |
| **FileManager** | Full file manager (folder tree, browser, toolbar, upload modal, properties) |
| **SufiAbpFileCard** | Card for a single file (thumbnail, actions) |
| **SufiAbpFileThumbnail** | Thumbnail with overlay info |
| **SufiAbpStorageQuotaMeter** | Visual storage quota meter |
| **SufiAbpUploadProgress** | Upload progress bar |
| **SufiAbpFileStructureTable** | Table for managing file structures |

### Public (SufiChain.SufiAbp.FileManager.Blazor.Public)

| Component | Description |
|-----------|-------------|
| **SufiAbpFileImage** | Responsive image with srcset, lazy loading, alt |
| **SufiAbpFileVideo** | HTML5 video player |
| **SufiAbpFileDownloadLink** | Download link with icon and size |
| **SufiAbpFileGallery** | Public gallery component |
| **SufiAbpFileAttachmentList** | List of file attachments (download links) |

---

## Admin Components

### SufiAbpFileUploader

Multi-file uploader with drag-drop, progress tracking, and structure validation.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Upload`

```razor
@using SufiChain.SufiAbp.FileManager.Blazor.Components.Upload
@using SufiChain.SufiAbp.FileManager.FileItems

<SufiAbpFileUploader 
    StructureKey="Product.Gallery"
    EntityType="Product"
    EntityId="@ProductId"
    AutoConfirm="true"
    AllowMultiple="true"
    ShowStorageQuota="true"
    OnUploadCompleted="@HandleUploadCompleted" />

@code {
    private Guid ProductId { get; set; }

    private void HandleUploadCompleted(List<FileItemDto> files)
    {
        // files uploaded successfully
    }
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string | — | File structure key for validation |
| EntityType | string | — | Associate files with entity type |
| EntityId | Guid? | — | Associate files with entity ID |
| FolderId | Guid? | — | Upload to folder |
| AutoConfirm | bool | false | Auto-confirm (or upload as temp) |
| AllowMultiple | bool | true | Allow multiple files |
| ShowStorageQuota | bool | true | Show quota meter |
| ShowStructureSelector | bool | false | Show structure dropdown |
| DropZoneTitle | string | "Drag & Drop Files Here" | |
| DropZoneDescription | string | "or click Browse Files to select" | |
| OnUploadCompleted | EventCallback&lt;List&lt;FileItemDto&gt;&gt; | — | Fired when all uploads complete |
| OnUploadError | EventCallback&lt;string&gt; | — | Fired on error |

---

### SufiAbpQuickImageUploader

Simple single-image uploader with preview.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Upload`

```razor
<SufiAbpQuickImageUploader 
    StructureKey="User.Avatar"
    AutoConfirm="true"
    Placeholder="Max 5MB, JPG/PNG/WebP"
    OnImageUploaded="@HandleImageUploaded" />

@code {
    private void HandleImageUploaded(FileItemDto file)
    {
        // file.Id, file.ThumbnailBlobName, etc.
    }
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string | — | File structure key |
| EntityType | string | — | Entity type |
| EntityId | Guid? | — | Entity ID |
| AutoConfirm | bool | true | Auto-confirm |
| Placeholder | string | "Max 5MB, JPG/PNG/WebP" | Placeholder text |
| MaxFileSize | long | 5MB | Max file size (bytes) |
| OnImageUploaded | EventCallback&lt;FileItemDto&gt; | — | Fired when image uploaded |

---

### SufiAbpFileBrowser

Grid/list file browser with search, filters, selection, and bulk operations.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Browser`

```razor
<SufiAbpFileBrowser 
    StructureKey="Product.Gallery"
    EntityType="Product"
    EntityId="@ProductId"
    OnFileSelected="@HandleFileSelected" />

@code {
    private void HandleFileSelected(FileItemDto file)
    {
        // handle selection
    }
}
```

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| StructureKey | string | Filter by structure |
| EntityType | string | Filter by entity type |
| EntityId | Guid? | Filter by entity ID |
| OnFileSelected | EventCallback&lt;FileItemDto&gt; | Fired on file click |

---

### SufiAbpFileGallery

Gallery view with search, filter, pagination, and optional selection.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Gallery`

```razor
<SufiAbpFileGallery 
    StructureKey="Product.Gallery"
    EntityType="Product"
    EntityId="@ProductId"
    Selectable="true"
    PageSize="12"
    OnFileSelected="@HandleFileSelected"
    OnSelectionChanged="@HandleSelectionChanged" />

@code {
    private void HandleFileSelected(FileItemDto file) { }
    private void HandleSelectionChanged(List<Guid> selectedIds) { }
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string | — | Filter by structure |
| EntityType | string | — | Filter by entity type |
| EntityId | Guid? | — | Filter by entity ID |
| Selectable | bool | false | Enable selection checkboxes |
| ShowActions | bool | true | Show view/edit/delete buttons |
| PageSize | int | 12 | Items per page |
| OnFileSelected | EventCallback&lt;FileItemDto&gt; | — | Fired on file click |
| OnSelectionChanged | EventCallback&lt;List&lt;Guid&gt;&gt; | — | Fired when selection changes |

---

### SufiAbpFileSelector

Modal dialog for browsing and selecting existing files.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Gallery`

```razor
<SbButton OnClick="@(() => _selectorOpen = true)">Select File</SbButton>

<SufiAbpFileSelector 
    @bind-Open="_selectorOpen"
    StructureKey="Product.Gallery"
    OnFileSelected="@HandleFileSelected" />

@code {
    private bool _selectorOpen = false;

    private void HandleFileSelected(FileItemDto? file)
    {
        if (file != null)
        {
            // use file.Id
        }
    }
}
```

---

### FileManager

Full-featured file manager with folder tree, browser, toolbar, upload, and properties panel.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.FileManager`

```razor
<FileManager 
    ShowFolderTree="true"
    ShowTenantTree="false"
    DefaultViewMode="ViewMode.Grid"
    OnFileSelected="@HandleFileSelected" />

@code {
    private void HandleFileSelected(FileItemDto file)
    {
        // open preview, etc.
    }
}
```

**Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ShowFolderTree | bool | true | Show folder tree sidebar |
| ShowTenantTree | bool | false | Show tenant root folders |
| DefaultViewMode | ViewMode | Grid | Grid or List |
| OnFileSelected | EventCallback&lt;FileItemDto&gt; | — | Fired on file click |

---

### SufiAbpFileCard

Card for displaying a single file with thumbnail and actions.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Common`

```razor
<SufiAbpFileCard 
    FileItem="@file"
    ShowDetails="true"
    ShowActions="true"
    Selectable="true"
    OnView="@HandleView"
    OnEdit="@HandleEdit"
    OnDelete="@HandleDelete"
    OnSelect="@HandleSelect" />
```

---

### SufiAbpFileThumbnail

Thumbnail image with optional title and badge.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Common`

```razor
<SufiAbpFileThumbnail 
    FileItem="@file"
    Width="150"
    Height="150"
    Rounded="true"
    ShowTitle="true" />
```

---

### SufiAbpStorageQuotaMeter

Visual meter for storage quota usage.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Common`

```razor
<SufiAbpStorageQuotaMeter />
```

Automatically fetches and displays current tenant's storage quota.

---

### SufiAbpUploadProgress

Progress bar for a single file upload.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Common`

```razor
<SufiAbpUploadProgress 
    FileName="@fileName"
    Progress="@progress"
    Status="@status"
    ErrorMessage="@errorMessage" />
```

---

### SufiAbpFileStructureTable

Table for viewing and managing file structures.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Components.Structures`

```razor
<SufiAbpFileStructureTable />
```

---

## Public Components

These components are for displaying files to end users (non-admin).

### SufiAbpFileImage

Responsive image with lazy loading and srcset.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Public.Components`

```razor
@using SufiChain.SufiAbp.FileManager.Blazor.Public.Components

<SufiAbpFileImage FileId="@imageId" Alt="Product image" Width="400" />
```

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| FileId | Guid | File item ID |
| Alt | string | Alt text |
| Width | int? | Width (px) |
| Height | int? | Height (px) |
| UseThumbnail | bool | Use thumbnail instead of original |
| LazyLoad | bool | Enable lazy loading |

---

### SufiAbpFileVideo

HTML5 video player.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Public.Components`

```razor
<SufiAbpFileVideo FileId="@videoId" AutoPlay="false" Controls="true" />
```

---

### SufiAbpFileDownloadLink

Download link with file icon and size.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Public.Components`

```razor
<SufiAbpFileDownloadLink FileId="@fileId" ShowSize="true" />
```

---

### SufiAbpFileGallery (Public)

Gallery for displaying images/files to users.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Public.Components`

```razor
<SufiAbpFileGallery FileIds="@fileIds" Columns="3" />
```

---

### SufiAbpFileAttachmentList

List of attachments with download links.

**Namespace:** `SufiChain.SufiAbp.FileManager.Blazor.Public.Components`

```razor
<SufiAbpFileAttachmentList FileIds="@attachmentIds" />
```

---

## URL Resolution (Tiered Apps)

When Blazor and API run on different origins, thumbnail/download URLs must point to the API. This is handled automatically via **IFileItemUrlProvider** (registered by Blazor.Public).

**Blazor app appsettings:**

```json
{
  "RemoteServices": {
    "SufiAbpFileManager": { "BaseUrl": "https://localhost:44305/" }
  }
}
```

Components and helpers use **IFileItemUrlProvider** to build correct URLs. See [Configuration – Tiered applications](configuration.md#3-tiered-applications-blazor-and-api-on-different-urls).

---

## Styling

Components use SufiBlazor (`Sb*`) and standard CSS classes. Override as needed:

```css
/* Uploader */
.sabp-file-uploader { }
.sabp-file-uploader-dropzone { }

/* Gallery */
.sabp-file-gallery { }
.sabp-file-gallery-grid { }

/* Card */
.sabp-file-card { }
.sabp-file-card-thumbnail { }

/* Thumbnail */
.sabp-file-thumbnail { }
```

---

## Localization

Components use ABP localization via `SufiAbpFileManagerResource`. Override in your localization JSON:

```json
{
  "culture": "en",
  "texts": {
    "DropFilesOrClick": "Drop files here or click to browse",
    "UploadProgress": "Uploading {0} of {1}",
    "FileTooLarge": "File size exceeds maximum: {0}",
    "InvalidFileType": "File type not allowed"
  }
}
```

---

## Best Practices

1. **Always use StructureKey** for validation and processing rules.
2. **Use AutoConfirm=false** for forms with validation; call **ConfirmAsync** on submit.
3. **Show SufiAbpStorageQuotaMeter** when users upload frequently.
4. **Use SufiAbpFileImage/SufiAbpFileVideo** (Blazor.Public) for public-facing display.
5. **Set RemoteServices:SufiAbpFileManager:BaseUrl** in tiered setups.

---

## Troubleshooting

| Issue | Check |
|-------|-------|
| **Thumbnail 404** | Tiered: set `RemoteServices:SufiAbpFileManager:BaseUrl` in Blazor app |
| **Upload fails** | Structure allowed types/sizes; `MaxUploadFileSizeMB`; quota |
| **Component not rendering** | Add `@using SufiChain.SufiAbp.FileManager.Blazor.Components.*` in `_Imports.razor` |

---

## See Also

- [Configuration](configuration.md)
- [Integration guide](integration-guide.md)
- [API reference](api-reference.md)
