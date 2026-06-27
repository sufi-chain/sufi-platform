# File Manager Markdown Integration

The File Manager public Blazor module integrates with markdown-capable editors through a single shared gallery host and toolbar contributor path.

## Components

### FileGalleryHost

Place `FileGalleryHost` once on the page or layout that contains your editor.

```razor
@using SufiChain.SufiAbp.FileManager.Blazor.Public.Editors

<FileGalleryHost />
```

### FileManagerMarkdownToolbarContributor

When `UseToolbarContributors="true"` is enabled, the public file manager integration adds:

| Button | Action |
|--------|--------|
| Insert Image | Inserts `![alt](url)` markdown |
| Attach File | Inserts `[filename](url)` markdown |

## Setup

1. Add the public file manager integration module:

```csharp
[DependsOn(typeof(SufiAbpFileManagerBlazorPublicModule))]
```

2. Add `FileGalleryHost` and use `SbMarkEditor` or `SbMarkdownEditor`:

```razor
@using SufiChain.SufiAbp.FileManager.Blazor.Public.Editors
@using SufiChain.SufiBlazor.Components.Forms

<FileGalleryHost />

<SbMarkEditor @bind-Value="_content"
              MinHeight="400px"
              Mode="SufiChain.SufiBlazor.Contracts.Editors.SbMarkEditorMode.Markdown"
              UseToolbarContributors="true"
              EnableMermaid="true"
              EnableHighlight="true" />
```

3. Keep `UseToolbarContributors="true"` enabled so the contributor can register the gallery actions.

## Dependencies

- `SufiChain.SufiAbp.FileManager.Blazor.Public`
- `SufiChain.SufiBlazor`
