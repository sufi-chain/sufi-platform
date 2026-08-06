# FileBrowser Component

A flat file list component with search, filtering, sorting, grid/list views, upload, and bulk delete. Unlike `FileManager`, it has **no folder tree**—ideal for entity-scoped files (e.g., product images) or simple file galleries.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| StructureKey | string? | null | Required. File structure key (e.g., `FileStructureKeys.General`) |
| EntityType | string? | null | Optional. Entity type to scope files (e.g., `Product`) |
| EntityId | Guid? | null | Optional. Entity ID to scope files |
| OnFileSelected | EventCallback\<FileItemDto\> | - | Callback when a file is selected (e.g., clicked) |

## Features

- **Search** – Filter by name (Enter to apply)
- **Type filter** – All types, Image, Video, Document, Audio
- **Sort** – Newest/Oldest, Name A-Z/Z-A, Size
- **Public only** – Checkbox to show only public files
- **Grid/List view** – Toggle between grid and table
- **Upload** – Open upload modal
- **Bulk delete** – Select multiple files and delete
- **Storage quota** – Shows usage meter when available

## Usage

### Basic (General structure)

```razor
<FileBrowser StructureKey="@SufiChain.SufiPlatform.FileManager.Configuration.FileStructureKeys.General" />
```

Or with a using: `@using SufiChain.SufiPlatform.FileManager.Configuration`:
```razor
<FileBrowser StructureKey="@FileStructureKeys.General" />
```

### Entity-scoped (e.g., Product images)

```razor
<FileBrowser
    StructureKey="@FileStructureKeys.General"
    EntityType="Product"
    EntityId="@productId"
    OnFileSelected="@HandleFileSelected" />
```

## Requirements

- File Manager Blazor module configured
- Appropriate permissions (e.g., `FileManagerPermissions.FileItems.Default`)
- A valid file structure (e.g., `General`) configured in `FileManagerOptions`
