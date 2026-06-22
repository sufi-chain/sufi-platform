# File Manager Markdown Editor Integration

The File Manager module provides integration with `SbMarkdownEditor`, allowing users to insert images from the file gallery and attach files from the markdown editor toolbar.

## Components

### FileGalleryHost

Reuse the host from the Rich Text Editor integration package. Place it once on the page or layout that uses `SbMarkdownEditor` with file manager toolbar contributors.

```razor
<FileGalleryHost />
```

### FileManagerMarkdownToolbarContributor

Adds toolbar buttons to `SbMarkdownEditor` when `UseToolbarContributors="true"` and `FileGalleryHost` is present on the page:

| Button | Icon | Action |
|--------|------|--------|
| Insert Image from Gallery | 📁 | Inserts `![alt](url)` markdown at the cursor |
| Attach File | 📎 | Inserts `[filename](url)` download link at the cursor |

Buttons are hidden automatically when `FileGalleryHost` is not registered on the page.

## Setup

1. **Add both integration modules** to your application:

```csharp
[DependsOn(
    typeof(SufiAbpFileManagerRichTextEditorModule),
    typeof(SufiAbpFileManagerMarkdownEditorModule))]
```

2. **Add FileGalleryHost and SbMarkdownEditor** to your page:

```razor
@using SufiChain.SufiAbp.FileManager.RichTextEditor.Components

<FileGalleryHost />

<SbMarkdownEditor @bind-Value="_content"
                  MinHeight="400px"
                  UseToolbarContributors="true"
                  EnableMermaid="true"
                  EnableHighlight="true" />
```

3. Set `UseToolbarContributors="true"` so the markdown toolbar contributor can register gallery buttons.

## Dependencies

- **SufiChain.SufiAbp.FileManager.RichTextEditor** – `FileGalleryHost`, `IFileGalleryDialogService`
- **SufiChain.SufiAbp.FileManager.MarkdownEditor** – `FileManagerMarkdownToolbarContributor`
- **SufiChain.SufiBlazor** – `SbMarkdownEditor`
