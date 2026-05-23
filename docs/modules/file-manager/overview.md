# File Manager Overview

The File Manager module provides reusable file and media capabilities for Sufi Platform applications. It is one of the richer source areas in the repository and a good reference module when you need to understand how a business capability can span domain logic, APIs, storage providers, public UI, admin UI, and editor integration.

## What it enables

- file upload and storage workflows
- file structures for business-specific validation rules
- folder-based browsing and organization
- thumbnails and media processing
- public and administrative file presentation
- rich text editor integration

## How it fits the platform

Treat File Manager as a horizontal capability. Products and modules can depend on it instead of inventing separate upload rules, gallery components, asset selectors, or entity-linked file storage in each bounded context.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.FileManager.Blazor` for admin screens such as asset management and structures
- `SufiChain.SufiAbp.FileManager.Blazor.Public` for public-facing components such as galleries and download links
- `SufiChain.SufiAbp.FileManager.Application` and `.Application.Contracts` for the use cases and DTOs
- `SufiChain.SufiAbp.FileManager.Domain`, `.EntityFrameworkCore`, and `.MongoDB` for the storage model and provider-specific persistence
- `SufiChain.SufiAbp.FileManager.RichTextEditor` when the requirement touches editor integration
