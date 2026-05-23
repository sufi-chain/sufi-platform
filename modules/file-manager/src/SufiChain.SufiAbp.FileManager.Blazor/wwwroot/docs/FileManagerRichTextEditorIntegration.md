# File Manager Rich Text Editor Integration

The File Manager module provides integration with SbRichTextEditor, allowing users to insert images from the file gallery and attach files directly from the rich text editor toolbar.

## Components

### FileGalleryHost

A host component that provides the dialog UI for selecting files from the gallery. Place it once in your layout or page that uses SbRichTextEditor with the file manager toolbar.

```razor
<FileGalleryHost />
```

### FileGalleryDialog

Used internally by FileGalleryHost. The FileManagerToolbarContributor adds "Insert Image from Gallery" and "Attach File" buttons to the SbRichTextEditor toolbar when the Rich Text Editor module is loaded.

### FileGalleryDialogService

A scoped service that bridges the toolbar buttons and the FileGalleryHost dialogs. When the user clicks "Insert Image from Gallery" or "Attach File", the service opens the appropriate dialog and returns the selected file to the editor.

## Setup

1. **Add the Rich Text Editor module** to your application:

```csharp
[DependsOn(typeof(FileManagerRichTextEditorModule))]
```

2. **Add FileGalleryHost** to your page or layout:

```razor
<FileGalleryHost />

<SbFormField Label="Content">
    <SbRichTextEditor @bind-Value="_content"
                     Height="500px"
                     UseToolbarContributors="true" />
</SbFormField>
```

3. Set `UseToolbarContributors="true"` on SbRichTextEditor so the toolbar contributor can add the "Insert Image from Gallery" and "Attach File" buttons. The contributor is registered automatically when FileGalleryDialogService is available.

## Dependencies

- **SufiChain.SufiAbp.FileManager.Blazor** – Core file manager components
- **SufiChain.SufiAbp.FileManager.RichTextEditor** – Registers FileGalleryDialogService and FileManagerToolbarContributor
- **SufiChain.SufiBlazor** – SbRichTextEditor component
