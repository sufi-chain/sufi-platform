# Blob Storing Database Module

> **KB:** See workspace Obsidian vault `.obsidian/SufiAbp/Modules/Blob Storing Database.md` for verified capabilities.

## Code location

`sufi-abp/modules/blob-storing-database/`

## Quick facts

- Infrastructure-only blob provider storing blobs in the application database
- `DatabaseBlobProvider` implements ABP `IBlobProvider`
- No Application, Blazor, or HttpApi layers
- EF Core + MongoDB

## Start in source

- `SufiChain.SufiAbp.BlobStoring.Database.Domain` — `DatabaseBlob`, `DatabaseBlobProvider`
- Used by File Manager when database storage is configured
