# File Manager Markup Integration

The File Manager public Blazor module also supports markup and HTML editing through the same shared gallery host.

## Components

### FileGalleryHost

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors

<FileGalleryHost />
```

### Shared dialog service

`FileGalleryHost` works with the public `IFileGalleryDialogService` and toolbar contributors so pages do not need editor-specific integration packages anymore.

## Setup

1. Add the public module:

```csharp
[DependsOn(typeof(SufiFileManagerBlazorPublicModule))]
```

2. Add `FileGalleryHost` and use `SbMarkEditor` in markup mode:

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors
@using SufiChain.SufiBlazor.Components.Forms

<FileGalleryHost />

<SbFormField Label="Content">
    <SbMarkEditor @bind-Value="_content"
                  Height="500px"
                  Mode="SufiChain.SufiBlazor.Contracts.Editors.SbMarkEditorMode.Markup"
                  UseToolbarContributors="true" />
</SbFormField>
```

3. Keep `UseToolbarContributors="true"` enabled so shared file actions appear in the toolbar.

## Dependencies

- `SufiChain.SufiPlatform.FileManager.Blazor.Public`
- `SufiChain.SufiBlazor`