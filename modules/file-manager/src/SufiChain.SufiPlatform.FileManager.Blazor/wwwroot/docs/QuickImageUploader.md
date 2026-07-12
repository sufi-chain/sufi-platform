# QuickImageUploader Component

Simple single-image uploader with preview and remove button. Ideal for avatars, product main images, or any one-image picker. Uses `InputFile` (goes through SignalR), so keep files **&lt; ~5MB** for best results.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string? | null | File structure key (e.g., `FileStructureKeys.General`). When `ShowStructureSelector` is true, this is the initial/default selection. |
| ShowStructureSelector | bool | false | When true, shows a dropdown to let the user choose the target structure. Useful for general areas where the structure is not predetermined. |
| ShowFolderPathField | bool | false | When true and FolderPath not passed, show input for user to type folder path. Default false = upload to root. |
| FolderPath | string? | null | Target folder path (e.g., `/web/tourist`). When set, uploads go here; folders are created if missing. When not set, an optional input is shown. Empty = root. |
| EntityType | string? | null | Entity type to associate the image |
| EntityId | Guid? | null | Entity ID to associate the image |
| AutoConfirm | bool | true | Auto-confirm upload |
| Placeholder | string | "Max 5MB, JPG/PNG/WebP" | Placeholder text when empty |
| MaxFileSize | long | 5MB | Max file size in bytes |
| OnImageUploaded | EventCallback\<FileItemDto\> | - | Fired when upload completes |
| CssClass | string | "" | Additional CSS classes |

## Features

- **Single image** – One image at a time; new upload replaces previous
- **Preview** – Shows thumbnail when uploaded
- **Remove** – Button to delete and clear
- **Validation** – Size and type (image/*) checked before upload
- **Progress** – Progress bar during upload
- **Folder path** – Optional path (e.g., `/web/tourist`) with auto-create of missing folders. When `FolderPath` is passed, uploads go there. When `ShowFolderPathField` is true and `FolderPath` not passed, user can type a path in the Upload settings modal; empty = root
- **Upload settings** – When structure selector or folder path field is shown, a config (gear) button appears on the upload card. Click to open a modal to configure upload type and folder path

## Usage

### Basic

```razor
<QuickImageUploader StructureKey="@FileStructureKeys.General" />
```

### With callback and placeholder

```razor
<QuickImageUploader
    StructureKey="@FileStructureKeys.General"
    Placeholder="Max 5MB, JPG/PNG/WebP"
    OnImageUploaded="@HandleImageUploaded" />

@code {
    private void HandleImageUploaded(FileItemDto file)
    {
        // Use file.Id for product image, avatar, etc.
    }
}
```

### Entity-scoped (e.g., product main image)

```razor
<QuickImageUploader
    StructureKey="@FileStructureKeys.General"
    EntityType="Product"
    EntityId="@productId"
    OnImageUploaded="@(f => ProductImageId = f.Id)" />
```

### With structure selector (general areas)

Use when the component is placed in a general area and the user should choose the target structure:

```razor
<QuickImageUploader
    StructureKey="@FileStructureKeys.General"
    ShowStructureSelector="true"
    ShowFolderPathField="true"
    Placeholder="Max 5MB, JPG/PNG/WebP"
    OnImageUploaded="@HandleImageUploaded" />
```

Set `ShowFolderPathField="true"` to enable the Folder Path field in the Upload settings modal (opened via the gear icon). User can type a path like `/web/tourist`; empty = root.

### With fixed folder path

```razor
<QuickImageUploader
    StructureKey="@FileStructureKeys.General"
    FolderPath="/web/avatars"
    OnImageUploaded="@HandleImageUploaded" />
```

All uploads go to `/web/avatars`. No Folder Path input is shown.

## When to use

- **QuickImageUploader** – Single image, small (&lt;5MB): avatar, product main image
- **FileUploader** – Multiple files or large files: general uploads, batch import

## Requirements

- File Manager Blazor module configured
- Appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`)
