# SbAddButton

A specialized button for adding new items in builder interfaces. Supports a dropdown menu of options or direct add action. Perfect for visual builders and content editors.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string? | null | Button label text |
| Options | List<SbAddOption> | [] | Dropdown options for different add types |
| Variant | SbAddButtonVariant | Default | Button variant (Default, Primary, Outline, Ghost) |
| Size | SbAddButtonSize | Medium | Button size (Small, Medium, Large) |
| Disabled | bool | false | Whether the button is disabled |
| AriaLabel | string | "Add item" | Accessibility label |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnOptionSelected | EventCallback<SbAddOption> | Fired when a dropdown option is selected |
| OnAdd | EventCallback | Fired on click when no options are provided |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment? | Custom icon content (replaces default + icon) |

## SbAddOption Class

| Property | Type | Description |
|----------|------|-------------|
| Id | string | Unique identifier |
| Label | string | Display label |
| Description | string? | Optional description text |
| IconText | string? | Icon text (emoji or symbol) |
| Icon | RenderFragment? | Custom icon content |
| Disabled | bool | Whether the option is disabled |
| Data | object? | Custom data associated with option |

## CSS Classes

- `sb-add-button-container` - Container wrapper
- `sb-add-button` - Button element
- `sb-add-button--active` - Menu is open
- `sb-add-button__icon` - Icon container
- `sb-add-button__icon--rotated` - Rotated when open
- `sb-add-button__label` - Label text
- `sb-add-button__menu` - Dropdown menu
- `sb-add-button__option` - Menu option
- `sb-add-button__option-icon` - Option icon
- `sb-add-button__option-label` - Option label
- `sb-add-button__option-description` - Option description

## Examples

### Simple Add Button

```razor
<SbAddButton Label="Add Item" OnAdd="HandleAdd" />

@code {
    private void HandleAdd()
    {
        // Add new item
    }
}
```

### With Dropdown Options

```razor
<SbAddButton Label="Add Block" 
             Options="@blockOptions" 
             OnOptionSelected="HandleOptionSelected" />

@code {
    private List<SbAddOption> blockOptions = new()
    {
        new() { Id = "text", Label = "Text Block", IconText = "📝", Description = "Add a paragraph" },
        new() { Id = "image", Label = "Image", IconText = "🖼️", Description = "Add an image" },
        new() { Id = "video", Label = "Video", IconText = "🎬", Description = "Embed a video" },
        new() { Id = "divider", Label = "Divider", IconText = "—", Description = "Add a separator" }
    };
    
    private void HandleOptionSelected(SbAddOption option)
    {
        Console.WriteLine($"Selected: {option.Id}");
    }
}
```

### Variants

```razor
<SbAddButton Variant="SbAddButtonVariant.Default" Label="Default" OnAdd="Add" />
<SbAddButton Variant="SbAddButtonVariant.Primary" Label="Primary" OnAdd="Add" />
<SbAddButton Variant="SbAddButtonVariant.Outline" Label="Outline" OnAdd="Add" />
<SbAddButton Variant="SbAddButtonVariant.Ghost" Label="Ghost" OnAdd="Add" />
```

### Sizes

```razor
<SbAddButton Size="SbAddButtonSize.Small" Label="Small" OnAdd="Add" />
<SbAddButton Size="SbAddButtonSize.Medium" Label="Medium" OnAdd="Add" />
<SbAddButton Size="SbAddButtonSize.Large" Label="Large" OnAdd="Add" />
```

### Custom Icon

```razor
<SbAddButton OnAdd="Add">
    <Icon>
        <SbIcon Name="plus-circle" />
    </Icon>
</SbAddButton>
```

### Icon Only

```razor
<SbAddButton OnAdd="Add" AriaLabel="Add new item" />
```

### Page Builder Example

```razor
<div class="page-builder">
    @foreach (var block in blocks)
    {
        <div class="block">@block.Content</div>
    }
    
    <SbAddButton Label="Add Content Block"
                 Options="@contentOptions"
                 OnOptionSelected="AddBlock"
                 Variant="SbAddButtonVariant.Outline" />
</div>

@code {
    private List<Block> blocks = new();
    
    private List<SbAddOption> contentOptions = new()
    {
        new() { Id = "heading", Label = "Heading", IconText = "H", Description = "Add a heading" },
        new() { Id = "paragraph", Label = "Paragraph", IconText = "¶", Description = "Add text content" },
        new() { Id = "list", Label = "List", IconText = "•", Description = "Add a bulleted list" },
        new() { Id = "quote", Label = "Quote", IconText = "❝", Description = "Add a blockquote" },
        new() { Id = "code", Label = "Code", IconText = "</>", Description = "Add a code block" }
    };
    
    private void AddBlock(SbAddOption option)
    {
        blocks.Add(new Block { Type = option.Id, Content = $"New {option.Label}" });
    }
}
```
