# FileSelector Component

Modal dialog for browsing and selecting existing files. Supports single or multi-select. Call `ShowAsync()` to open the dialog. Includes a Structure dropdown to filter files by file structure (e.g., General, CMS); when `StructureKey` is passed, it is used as the initial selection.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string? | null | File structure key (e.g., `FileStructureKeys.General`). Used as initial filter when modal opens; user can change via Structure dropdown. |
| AllowMultiple | bool | false | Allow selecting multiple files |
| FilterFileType | FileType? | null | Restrict to a specific file type (Image, Video, Document, etc.) |
| OnFileSelected | EventCallback\<List\<FileItemDto\>\> | - | Fired when selection is confirmed |

## Methods

| Method | Description |
|--------|-------------|
| ShowAsync() | Opens the modal and loads files |
| Hide() | Closes the modal |

## Usage

### Single file selection

```razor
<SbButton OnClick="@OpenSelector">Select File</SbButton>

<FileSelector
    @ref="_fileSelectorRef"
    StructureKey="@FileStructureKeys.General"
    OnFileSelected="@HandleFileSelected" />

@code {
    private FileSelector? _fileSelectorRef;

    private async Task OpenSelector()
    {
        if (_fileSelectorRef != null)
            await _fileSelectorRef.ShowAsync();
    }

    private void HandleFileSelected(List<FileItemDto> files)
    {
        var file = files.FirstOrDefault();
        // use file
    }
}
```

### Multi-select, images only

```razor
<FileSelector
    @ref="_fileSelectorRef"
    StructureKey="@FileStructureKeys.General"
    AllowMultiple="true"
    FilterFileType="FileType.Image"
    OnFileSelected="@HandleFilesSelected" />
```

## Requirements

- File Manager Blazor module configured
- Appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`)
