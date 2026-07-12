# File Manager Architecture

The File Manager module is a strong reference implementation for a full ABP-style business module in Sufi.

## Projects

- `Application.Contracts`
- `Application`
- `Domain.Shared`
- `Domain`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Blazor.Public`
- `Blazor.Server`
- `Blazor.WebAssembly`
- `EntityFrameworkCore`
- `MongoDB`
- `RichTextEditor`
- `Demo`
- `test/*`

## Application areas

### File items

Application contracts and services under `FileItems` cover upload, listing, streaming, replacement, metadata access, and related file operations.

### File folders

Folder contracts and services support hierarchy, browsing, and folder-oriented organization.

### File structures

File structures define reusable validation and processing rules for uploads.

## UI composition

The Blazor admin package contains menu integration and pages for asset management, statistics, and structure management.

The public package provides reusable file presentation components.

The rich-text-editor package integrates media selection into editor workflows.

## Persistence

The module provides both EF Core and MongoDB packages, making it suitable for multiple host persistence choices.

## Why it matters

If you need a reference for how to build a substantial business module on Sufi Platform, File Manager is one of the best examples in the public source.
