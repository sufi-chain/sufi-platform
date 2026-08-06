# FileGallery Component

Gallery component for displaying and managing file items with grid/list views, search, filter, pagination, and lightbox preview.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string? | null | File structure key (e.g., `FileStructureKeys.General`) |
| EntityType | string? | null | Entity type to scope files (e.g., `Product`) |
| EntityId | Guid? | null | Entity ID to scope files |
| Selectable | bool | false | Enable selection checkboxes |
| ShowActions | bool | true | Show view/edit/delete buttons |
| PageSize | int | 12 | Items per page |
| OnFileSelected | EventCallback\<FileItemDto\> | - | Fired when a file is clicked |
| OnSelectionChanged | EventCallback\<List\<Guid\>\> | - | Fired when selection changes |
| CssClass | string | "" | Additional CSS classes |

## Features

- **Search** – Filter by name (click Search to apply)
- **Type filter** – All types, Image, Video, Document
- **Grid/List view** – Toggle between grid and table
- **Pagination** – Navigate through pages
- **Lightbox** – Click view to preview image/video in a modal
- **Selection** – Optional multi-select with `OnSelectionChanged`
- **Actions** – View, edit (coming soon), delete per file

## Usage

### Basic

```razor
<FileGallery StructureKey="@FileStructureKeys.General" />
```

### Entity-scoped with selection

```razor
<FileGallery
    StructureKey="@FileStructureKeys.General"
    EntityType="Product"
    EntityId="@productId"
    Selectable="true"
    PageSize="12"
    OnFileSelected="@HandleFileSelected"
    OnSelectionChanged="@HandleSelectionChanged" />

@code {
    private void HandleFileSelected(FileItemDto file) { }
    private void HandleSelectionChanged(List<Guid> selectedIds) { }
}
```

## Requirements

- File Manager Blazor module configured
- Appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`)
