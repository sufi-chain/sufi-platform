# File Manager API

The File Manager API surface spans multiple functional areas.

## Application-service areas

### File items

Key contracts and DTOs exist under `FileItems`, including:

- `IFileItemAppService`
- upload and replacement DTOs
- list/filter DTOs
- stream and metadata DTOs

### File folders

Key contracts and DTOs exist under `FileFolders`, including:

- `IFolderAppService`
- folder tree and contents DTOs
- folder statistics and input DTOs

### File structures

Key contracts and DTOs exist under `FileStructures`, including:

- `IFileStructureAppService`
- file structure DTOs

## HTTP API controllers

The module exposes controllers for:

- `FileItemController`
- `FileManagerController`
- `FileStructureController`
- `FolderController`

## Related technical docs

For deeper API detail, continue with:

- [API Reference](api-reference.md)
- [Integration Guide](integration-guide.md)
