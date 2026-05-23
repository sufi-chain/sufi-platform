# FileManager Component

A full-featured file browser component for managing and browsing files in the Sufi Platform file management system.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ShowFolderTree | bool | true | Whether to show the folder tree sidebar |
| ShowTenantTree | bool | false | Whether to show tenant roots in the folder tree |
| ShowPropertiesPanel | bool | false | Whether to show the properties panel for selected items |
| ShowPropertiesPanelChanged | EventCallback\<bool\> | - | Callback when properties panel visibility changes |
| AllowCreateFolders | bool | true | Whether users can create new folders |
| AllowDragDrop | bool | true | Whether drag-and-drop operations are enabled |
| AllowMultiSelect | bool | true | Whether multiple files/folders can be selected |
| InitialViewMode | FileViewMode | LargeIcons | Initial view mode (LargeIcons, SmallIcons, List, Tiles) |
| Class | string? | null | Additional CSS classes |
| OnFileOpen | EventCallback\<FileItemDto\> | - | Callback when a file is opened (e.g., double-click) |
| OnSelectionChanged | EventCallback\<List\<FileItemDto\>\> | - | Callback when selection changes |
| OnEditImage | EventCallback\<FileItemDto\> | - | Callback when image edit is requested |
| RefreshTrigger | int | 0 | Increment to force a refresh of the file list |

## Usage

```razor
<FileManager
    ShowFolderTree="true"
    ShowTenantTree="false"
    ShowPropertiesPanel="true"
    AllowCreateFolders="true"
    AllowDragDrop="true"
    AllowMultiSelect="true"
    InitialViewMode="FileViewMode.LargeIcons"
    OnFileOpen="@HandleFileOpen"
    OnSelectionChanged="@HandleSelectionChanged" />
```

## View Modes

- **LargeIcons** – Grid of large file icons
- **SmallIcons** – Grid of small file icons
- **List** – List view with details
- **Tiles** – Tile layout

## Requirements

The FileManager component requires the File Manager Blazor module to be configured and the user to have appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`).
