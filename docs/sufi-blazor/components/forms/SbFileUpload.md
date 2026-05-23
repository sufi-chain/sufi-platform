# SbFileUpload

A file upload component with drag-and-drop support, file type and size validation, and a list of selected files with remove support.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Accept | string? | null | Accepted file types (e.g., "image/*", ".pdf,.doc,.docx") |
| Multiple | bool | false | Whether to allow multiple files |
| MaxFileSizeBytes | long? | null | Maximum file size in bytes |
| MaxFiles | int? | null | Maximum number of files (when Multiple=true) |
| OnFilesSelected | EventCallback\<IReadOnlyList\<IBrowserFile\>\> | - | Callback when files are selected |
| OnFileRemoved | EventCallback\<IBrowserFile\> | - | Callback when a file is removed from the list |
| Disabled | bool | false | Whether the upload is disabled |
| DragText | RenderFragment? | null | Custom content for the drop zone (when null, default text is shown) |
| FileItemTemplate | RenderFragment\<IBrowserFile\>? | null | Custom template for each file in the list |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnFilesSelected | EventCallback\<IReadOnlyList\<IBrowserFile\>\> | Fired when files are selected |
| OnFileRemoved | EventCallback\<IBrowserFile\> | Fired when a file is removed from the list |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| DragText | RenderFragment | Custom content for the drop zone |
| FileItemTemplate | RenderFragment\<IBrowserFile\> | Custom template for each file item in the list |

## CSS Classes

- `sb-file-upload` - Base class
- `sb-file-upload__dropzone` - Drop zone area
- `sb-file-upload__dropzone--dragging` - Dragging over state
- `sb-file-upload__dropzone--disabled` - Disabled state
- `sb-file-upload__input` - Hidden file input
- `sb-file-upload__icon` - Upload icon
- `sb-file-upload__text` - Instruction text
- `sb-file-upload__list` - Selected files list
- `sb-file-upload__item` - Individual file item
- `sb-file-upload__item-icon` - File item icon
- `sb-file-upload__item-info` - File name and size wrapper
- `sb-file-upload__item-name` - File name
- `sb-file-upload__item-size` - File size
- `sb-file-upload__item-remove` - Remove button

## Examples

### Basic Usage

```razor
<SbFileUpload OnFilesSelected="HandleFiles" />
```

### Image Upload

```razor
<SbFileUpload Accept="image/*"
              MaxFileSizeBytes="@(5 * 1024 * 1024)"
              OnFilesSelected="HandleImages">
    <DragText>
        <SbStack Direction="SbStackDirection.Column" Gap="2" Align="SbAlign.Center">
            <SbIcon Name="image" Size="SbSize.Xl" />
            <SbText>Drag & drop images here</SbText>
            <SbText Size="SbSize.Sm" Color="SbColor.Muted">
                or click to browse (max 5MB)
            </SbText>
        </SbStack>
    </DragText>
</SbFileUpload>
```

### Multiple Files

```razor
<SbFileUpload Multiple="true"
              MaxFiles="10"
              Accept=".pdf,.doc,.docx"
              OnFilesSelected="HandleDocuments">
    <DragText>
        <SbText>Upload documents (up to 10 files)</SbText>
    </DragText>
</SbFileUpload>
```

### Disabled State

```razor
<SbFileUpload Disabled="true">
    <DragText>
        <SbText>Upload disabled</SbText>
    </DragText>
</SbFileUpload>
```
