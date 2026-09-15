# File Manager Markup Integration

The File Manager public Blazor module also supports HTML editing through the same shared gallery host.

## Components

### FileGalleryHost

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors

<FileGalleryHost />
```

### Shared dialog service

`FileGalleryHost` works with the public `IFileGalleryDialogService` and `SufiFileManagerEditorContributor` so pages do not need editor-specific integration packages.

## Setup

1. Add the public module:

```csharp
[DependsOn(typeof(SufiFileManagerBlazorPublicModule))]
```

2. Add `FileGalleryHost` and use `SbRichTextEditor` in HTML mode:

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors
@using SufiChain.SufiBlazor.Components.Forms
@using SufiChain.SufiBlazor.Contracts.Editors

<FileGalleryHost />

<SbFormField Label="Content">
    <SbRichTextEditor @bind-Value="_content"
                      Height="500px"
                      ContentFormat="SbContentFormat.Html"
                      UseToolbarContributors="true" />
</SbFormField>
```

3. Keep `UseToolbarContributors="true"` enabled so shared file actions appear in the toolbar.

## Dependencies

- `SufiChain.SufiPlatform.FileManager.Blazor.Public`
- `SufiChain.SufiBlazor`
