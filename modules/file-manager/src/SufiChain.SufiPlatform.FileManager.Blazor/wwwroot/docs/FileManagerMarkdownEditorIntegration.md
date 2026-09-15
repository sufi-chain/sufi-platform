# File Manager Markdown Integration

The File Manager public Blazor module integrates with markdown-capable editors through a single shared gallery host and toolbar contributor path.

## Components

### FileGalleryHost

Place `FileGalleryHost` once on the page or layout that contains your editor.

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors

<FileGalleryHost />
```

### SufiFileManagerEditorContributor

When `UseToolbarContributors="true"` is enabled, the public file manager integration adds:

| Button | Action |
|--------|--------|
| Insert Image | Inserts an image through `ISbEditorDocument` |
| Attach File | Inserts a file link through `ISbEditorDocument` |

## Setup

1. Add the public file manager integration module:

```csharp
[DependsOn(typeof(SufiFileManagerBlazorPublicModule))]
```

2. Add `FileGalleryHost` and use `SbRichTextEditor` with markdown content:

```razor
@using SufiChain.SufiPlatform.FileManager.Blazor.Public.Editors
@using SufiChain.SufiBlazor.Components.Forms
@using SufiChain.SufiBlazor.Contracts.Editors

<FileGalleryHost />

<SbRichTextEditor @bind-Value="_content"
                  MinHeight="400px"
                  ContentFormat="SbContentFormat.Markdown"
                  UseToolbarContributors="true" />
```

3. Keep `UseToolbarContributors="true"` enabled so the contributor can register the gallery actions.

## Dependencies

- `SufiChain.SufiPlatform.FileManager.Blazor.Public`
- `SufiChain.SufiBlazor`
