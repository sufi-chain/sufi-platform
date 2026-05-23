# File Manager Permissions

The module defines these permission areas:

| Permission | Purpose |
| --- | --- |
| `SufiAbpFileManager.FileItems` | Access file-item functionality |
| `SufiAbpFileManager.FileItems.Create` | Create/upload file items |
| `SufiAbpFileManager.FileItems.Update` | Update file items |
| `SufiAbpFileManager.FileItems.Delete` | Delete file items |
| `SufiAbpFileManager.FileStructures` | Access file-structure functionality |
| `SufiAbpFileManager.FileStructures.Create` | Create file structures |
| `SufiAbpFileManager.FileStructures.Update` | Update file structures |
| `SufiAbpFileManager.FileStructures.Delete` | Delete file structures |

## Notes

These permissions are defined in the application contracts layer and should be used to protect administrative file-management workflows.
