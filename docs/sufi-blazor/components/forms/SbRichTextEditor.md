# SbRichTextEditor

A rich text editor component for creating and editing formatted content.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | string? | null | The HTML content (two-way bindable) |
| ValueChanged | EventCallback\<string?\> | - | Callback when content changes |
| Mode | SbEditorMode | SbEditorMode.Html | Editor mode (e.g. Html) |
| ToolbarItems | IReadOnlyList\<SbEditorToolbarItem\>? | null | Custom toolbar items (when null, default toolbar is used) |
| Placeholder | string? | null | Placeholder text when empty |
| ReadOnly | bool | false | Whether the editor is read-only |
| Disabled | bool | false | Whether the editor is disabled |
| RightToLeft | bool | false | Whether the editor is RTL |
| Height | string? | "300px" | Editor content area height |
| HideToolbar | bool | false | When true, hides the toolbar |
| ShowCharacterCount | bool | false | Whether to show character count in footer |
| ShowWordCount | bool | false | Whether to show word count in footer |
| UseToolbarContributors | bool | false | Enable toolbar items from registered contributor services |
| IncludeDefaultToolbarItems | bool | true | Whether to include default toolbar items |
| Class | string? | null | Additional CSS classes |

Additional parameters for dialogs and accessibility (e.g. ToolbarAriaLabel, EditorAriaLabel, LinkDialogTitle, ImageDialogTitle, WordCountFormat, CharacterCountFormat, and various link/image dialog labels) are available; when null or not set, defaults are used.

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<string?\> | Fired when the content changes |
| OnChange | EventCallback\<string?\> | Fired when content changes (receives current HTML) |
| OnFocus | EventCallback | Fired when editor gains focus |
| OnBlur | EventCallback | Fired when editor loses focus |

## CSS Classes

- `sb-editor` - Base class
- `sb-editor__toolbar` - Toolbar container
- `sb-editor__toolbar-separator` - Toolbar separator
- `sb-editor__toolbar-select` - Toolbar dropdown (e.g. headings)
- `sb-editor__toolbar-btn` - Toolbar button
- `sb-editor__toolbar-btn--active` - Active format button
- `sb-editor__toolbar-icon` - Toolbar icon
- `sb-editor__content` - Editable content area
- `sb-editor__footer` - Footer (word/character count)
- `sb-editor__count` - Count display
- `sb-editor__link-dialog` - Link/Image dialog container
- `sb-editor__link-dialog-field` - Dialog field
- `sb-editor__link-input` - Dialog input
- `sb-editor--disabled` - Disabled state
- `sb-editor--readonly` - Read-only state
- `sb-editor--rtl` - Right-to-left state

## Examples

### Basic Usage

```razor
<SbRichTextEditor @bind-Value="content" />
```

### With Placeholder

```razor
<SbRichTextEditor @bind-Value="description" 
                  Placeholder="Enter description..." />
```

### Custom Height

```razor
<SbRichTextEditor @bind-Value="articleContent" 
                  Height="400px" />
```

### Without Toolbar

```razor
<SbRichTextEditor @bind-Value="note" 
                  HideToolbar="true"
                  Height="200px" />
```

### Read-Only Display

```razor
<SbRichTextEditor Value="@displayContent" ReadOnly="true" />
```

### With Form Field

```razor
<SbFormField Label="Description" Required="true">
    <SbRichTextEditor @bind-Value="model.Description" 
                      Placeholder="Enter a detailed description..." />
    <SbFieldError For="() => model.Description" />
</SbFormField>
```

## Toolbar Extensibility

The `SbRichTextEditor` supports extending the toolbar with custom buttons from external modules using the contributor pattern.

### Enabling Toolbar Contributors

To use toolbar contributors, set `UseToolbarContributors="true"` on the component:

```razor
<SbRichTextEditor @bind-Value="content" 
                  UseToolbarContributors="true" />
```

### Implementing a Toolbar Contributor

Create a class that implements `IRteToolbarContributor`:

```csharp
using SufiChain.SufiBlazor.Contracts.Editors;

public class MediaToolbarContributor : IRteToolbarContributor
{
    // Items with lower Order appear first. Default items use 0-100.
    // Use > 100 to add after defaults, negative values to add before.
    public int Order => 150;

    public Task ConfigureToolbarAsync(RteToolbarContext context)
    {
        // Add an "Insert Media" button
        context.Items.Add(new RteToolbarContributedItem
        {
            Id = "insert-media",
            Label = "Insert Media",
            Icon = "image",
            Tooltip = "Insert media from library",
            Group = "insert",  // Groups: history, heading, formatting, list, alignment, insert, blocks, custom, actions
            Order = 10,
            OnClickAsync = async (actionContext) =>
            {
                // Access services via DI
                var mediaService = actionContext.ServiceProvider.GetService<IMediaService>();
                
                // Get current selection if needed
                var selection = await actionContext.GetSelectionAsync?.Invoke()!;
                
                // Insert HTML at cursor position
                var html = "<img src=\"/media/example.jpg\" alt=\"Example\" />";
                await actionContext.InsertHtmlAsync?.Invoke(html)!;
            },
            // Conditionally show/hide
            IsVisible = () => true,
            // Conditionally enable/disable
            IsEnabled = () => true
        });

        return Task.CompletedTask;
    }
}
```

### Registering Toolbar Contributors

Register your contributor in `Program.cs` or your module's service configuration:

```csharp
// Method 1: Using the generic extension method
services.AddSufiBlazor();
services.AddRteToolbarContributor<MediaToolbarContributor>();

// Method 2: Multiple contributors
services.AddSufiBlazor();
services.AddRteToolbarContributor<MediaToolbarContributor>();
services.AddRteToolbarContributor<EmojiToolbarContributor>();

// Method 3: Using RteToolbarOptions configuration
services.AddSufiBlazor(options =>
{
    options.AddContributor<MediaToolbarContributor>();
    options.AddContributor<EmojiToolbarContributor>();
    
    // Optionally disable default toolbar items
    options.IncludeDefaultItems = false;
    
    // Customize group order
    options.GroupOrder = new List<string>
    {
        "formatting",
        "insert",
        "custom",
        "actions"
    };
});
```

### RteToolbarActionContext

The `OnClickAsync` callback receives an `RteToolbarActionContext` with useful actions:

| Property | Type | Description |
|----------|------|-------------|
| EditorId | string | The unique editor instance ID |
| ServiceProvider | IServiceProvider | DI service provider |
| InsertHtmlAsync | Func\<string, Task\> | Insert HTML at cursor |
| InsertImageAsync | Func\<string, string?, Task\> | Insert image (src, alt) |
| InsertLinkAsync | Func\<string, string?, Task\> | Insert link (url, text) |
| GetSelectionAsync | Func\<Task\<string?\>\> | Get selected text |
| GetHtmlAsync | Func\<Task\<string?\>\> | Get editor HTML content |

### Toolbar Groups

Items are organized into groups. Use the `Group` property to place your item:

| Group | Description |
|-------|-------------|
| history | Undo/Redo buttons |
| heading | Header formatting (H1-H6) |
| formatting | Bold, Italic, Underline, etc. |
| list | Ordered/Unordered lists |
| alignment | Text alignment |
| insert | Links, Images, Files |
| blocks | Blockquote, Code blocks |
| custom | Default group for contributed items |
| actions | Clear formatting, etc. |

### Complete Example: Emoji Picker Contributor

```csharp
public class EmojiToolbarContributor : IRteToolbarContributor
{
    public int Order => 200;

    public Task ConfigureToolbarAsync(RteToolbarContext context)
    {
        var emojis = new[] { "😀", "😊", "👍", "❤️", "🎉" };

        foreach (var (emoji, index) in emojis.Select((e, i) => (e, i)))
        {
            context.Items.Add(new RteToolbarContributedItem
            {
                Id = $"emoji-{index}",
                Label = emoji,
                Tooltip = $"Insert {emoji}",
                Group = "insert",
                Order = 100 + index,
                OnClickAsync = async (actionContext) =>
                {
                    await actionContext.InsertHtmlAsync?.Invoke(emoji)!;
                }
            });
        }

        return Task.CompletedTask;
    }
}
```
