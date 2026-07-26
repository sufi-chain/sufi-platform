# Blob Storing Database Module

Infrastructure-only blob storage backend that persists blobs in the application database (EF Core or MongoDB). Implements ABP `IBlobProvider` as `DatabaseBlobProvider`. Used by File Manager when database storage is selected. No Application, Blazor, or HttpApi layers.

## Code location

`sufi-platform/modules/blob-database/`

## Packages

Package segment: **`BlobDatabase`** (`SufiChain.SufiPlatform.BlobDatabase.*`).

| Layer | Project |
|-------|---------|
| Domain.Shared | `SufiChain.SufiPlatform.BlobDatabase.Domain.Shared` |
| Domain | `SufiChain.SufiPlatform.BlobDatabase.Domain` |
| EntityFrameworkCore | `SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore` |
| MongoDB | `SufiChain.SufiPlatform.BlobDatabase.MongoDB` |

## Capabilities

- `DatabaseBlob` and `DatabaseBlobContainer` entities
- `DatabaseBlobProvider` — save, get, delete, exists
- Multi-tenant blob storage via `ICurrentTenant`
- Included in the default CLI module registry for new solutions

## Related

- [File Manager](../file-manager/index.md)
- Framework S3 provider: `SufiChain.SufiPlatform.BlobStoring.S3Provider`
- [Package Map](../../reference/package-map.md)
