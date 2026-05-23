# FileUploader Component

Full-featured file uploader with drag-and-drop, Browse Files button, per-file progress, and storage quota display. Uses **direct HTTP upload** (bypasses SignalR) to avoid circuit timeout with large files.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string? | null | File structure key (e.g., `FileStructureKeys.General`) |
| EntityType | string? | null | Entity type to associate files (e.g., `Product`) |
| EntityId | Guid? | null | Entity ID to associate files |
| FolderPath | string? | null | Target folder path (e.g., `/web/tourist`). When set, uploads go here; folders are created if missing. When not set, an optional input is shown. Empty = root. |
| AllowMultiple | bool | true | Allow selecting multiple files |
| AutoConfirm | bool | false | Auto-confirm uploads |
| ShowStorageQuota | bool | true | Show storage quota meter |
| ShowStructureSelector | bool | false | Show dropdown to choose file structure |
| ShowFolderPathField | bool | false | When true and FolderPath not passed, show input for user to type folder path. Default false = upload to root. |
| DropZoneTitle | string | "Drag & Drop Files Here" | Title text |
| DropZoneDescription | string | "or click Browse Files to select" | Description text |
| OnUploadCompleted | EventCallback\<List\<FileItemDto\>\> | - | Fired when upload(s) complete |
| OnUploadError | EventCallback\<string\> | - | Fired on upload error |
| CssClass | string | "" | Additional CSS classes |

## Features

- **Drag-and-drop** – Drop zone with visual feedback (note: drop handling may require JS)
- **Browse Files** – Click to open native file picker
- **Per-file progress** – Each file shows upload progress and status
- **Structure-based** – Respects file structure (allowed types, max size, dimensions)
- **Storage quota** – Optional meter showing usage
- **Structure selector** – Optional dropdown to pick target structure
- **Folder path** – Optional path (e.g., `/web/tourist`) with auto-create of missing folders. When `FolderPath` is passed, uploads go there. When `ShowFolderPathField` is true and `FolderPath` not passed, user can type a path in the Upload settings modal; empty = root
- **Upload settings** – When structure selector or folder path field is shown, a config (gear) button appears on the upload card. Click to open a modal to configure upload type and folder path
- **HTTP upload** – Large files upload via HTTP, avoiding SignalR limits

## Usage

### Basic

```razor
<FileUploader StructureKey="@FileStructureKeys.General" />
```

### With callbacks, structure selector, and folder path field

```razor
<FileUploader
    StructureKey="@FileStructureKeys.General"
    ShowStorageQuota="true"
    ShowStructureSelector="true"
    ShowFolderPathField="true"
    OnUploadCompleted="@HandleUploadCompleted"
    OnUploadError="@HandleUploadError" />

@code {
    private void HandleUploadCompleted(List<FileItemDto> files) { }
    private void HandleUploadError(string message) { }
}
```

Set `ShowFolderPathField="true"` to enable the Folder Path field in the Upload settings modal (opened via the gear icon on the upload card). User can type a path like `/web/tourist`; empty = root. Folders are created automatically if missing.

### With fixed folder path

```razor
<FileUploader
    StructureKey="@FileStructureKeys.General"
    FolderPath="/web/tourist"
    OnUploadCompleted="@HandleUploadCompleted" />
```

All uploads go to `/web/tourist`. No Folder Path input is shown. Missing folders are created automatically.

### Entity-scoped

```razor
<FileUploader
    StructureKey="@FileStructureKeys.General"
    EntityType="Product"
    EntityId="@productId"
    FolderPath="/products/images" />
```

## Requirements

- File Manager Blazor module configured
- JavaScript interop for file input handling
- Appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`)
